using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using NetwiseRecruitmentTask;
using NetwiseRecruitmentTask.Models;
using NetwiseRecruitmentTask.Services;

[TestClass]
public class WorkerTests
{
    private Mock<IHostApplicationLifetime> _hostApplicationLifetimeMock = null!;
    private Mock<ICatFactApiService> _catFactApiServiceMock = null!;
    private Mock<IFilesystemService> _filesystemServiceMock = null!;
    private Mock<IConsoleService> _consoleServiceMock = null!;
    private Mock<ILogger<Worker>> _loggerMock = null!;

    [TestInitialize]
    public void Setup()
    {
        _hostApplicationLifetimeMock = new Mock<IHostApplicationLifetime>();
        _catFactApiServiceMock = new Mock<ICatFactApiService>();
        _filesystemServiceMock = new Mock<IFilesystemService>();
        _consoleServiceMock = new Mock<IConsoleService>();
        _loggerMock = new Mock<ILogger<Worker>>();
    }

    private Worker CreateSut(CommandLineOptions? options = null) => new(
        _hostApplicationLifetimeMock.Object,
        _catFactApiServiceMock.Object,
        _filesystemServiceMock.Object,
        _consoleServiceMock.Object,
        _loggerMock.Object,
        options ?? new CommandLineOptions(false, null));

    [TestMethod]
    public async Task RunLoopAsync_OnStart_EnsuresFileExists()
    {
        _consoleServiceMock
            .Setup(c => c.ReadLineAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync("exit");

        var sut = CreateSut();

        await sut.RunLoopAsync(CancellationToken.None);

        _filesystemServiceMock.Verify(
            f => f.EnsureFileExist(),
            Times.Once);
    }

    [TestMethod]
    public async Task RunLoopAsync_UserTypesExit_StopsLoopAndStopsApplication()
    {
        _consoleServiceMock
            .Setup(c => c.ReadLineAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync("exit");

        var sut = CreateSut();

        await sut.RunLoopAsync(CancellationToken.None);

        _catFactApiServiceMock.Verify(
            s => s.GetCatFactAsync(It.IsAny<CancellationToken>()),
            Times.Never);

        _hostApplicationLifetimeMock.Verify(
            h => h.StopApplication(),
            Times.Once);
    }

    [TestMethod]
    public async Task RunLoopAsync_UserTypesExit_CaseInsensitive_StopsLoop()
    {
        _consoleServiceMock
            .Setup(c => c.ReadLineAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync("ExIt");

        var sut = CreateSut();

        await sut.RunLoopAsync(CancellationToken.None);

        _hostApplicationLifetimeMock.Verify(
            h => h.StopApplication(),
            Times.Once);
    }

    [TestMethod]
    public async Task RunLoopAsync_UserPressesEnter_FetchesFactAndAppendsToFile()
    {
        var fact = new CatFact
        {
            Fact = "Cats sleep a lot.",
            Length = 17
        };

        _consoleServiceMock
            .SetupSequence(c => c.ReadLineAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(string.Empty)
            .ReturnsAsync("exit");

        _catFactApiServiceMock
            .Setup(s => s.GetCatFactAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(fact);

        var sut = CreateSut();

        await sut.RunLoopAsync(CancellationToken.None);

        _catFactApiServiceMock.Verify(
            s => s.GetCatFactAsync(It.IsAny<CancellationToken>()),
            Times.Once);

        _filesystemServiceMock.Verify(
            f => f.AppendLineAsync(
                $"Fact: {fact.Fact} length: {fact.Length}",
                It.IsAny<CancellationToken>()),
            Times.Once);

        _consoleServiceMock.Verify(
            c => c.WriteLine(fact.Fact),
            Times.Once);
    }

    [TestMethod]
    public async Task RunLoopAsync_UserTypesOnce_FetchesSingleFact()
    {
        var fact = new CatFact
        {
            Fact = "Cats sleep a lot.",
            Length = 17
        };

        _consoleServiceMock
            .SetupSequence(c => c.ReadLineAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync("once")
            .ReturnsAsync("exit");

        _catFactApiServiceMock
            .Setup(s => s.GetCatFactAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(fact);

        var sut = CreateSut();

        await sut.RunLoopAsync(CancellationToken.None);

        _catFactApiServiceMock.Verify(
            s => s.GetCatFactAsync(It.IsAny<CancellationToken>()),
            Times.Once);

        _filesystemServiceMock.Verify(
            f => f.AppendLineAsync(
                $"Fact: {fact.Fact} length: {fact.Length}",
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [TestMethod]
    public async Task RunLoopAsync_UserTypesCount_FetchesRequestedNumberOfFacts()
    {
        var fact = new CatFact
        {
            Fact = "Test fact",
            Length = 9
        };

        _consoleServiceMock
            .SetupSequence(c => c.ReadLineAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync("count 3")
            .ReturnsAsync("exit");

        _catFactApiServiceMock
            .Setup(s => s.GetCatFactAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(fact);

        var sut = CreateSut();

        await sut.RunLoopAsync(CancellationToken.None);

        _catFactApiServiceMock.Verify(
            s => s.GetCatFactAsync(It.IsAny<CancellationToken>()),
            Times.Exactly(3));

        _filesystemServiceMock.Verify(
            f => f.AppendLineAsync(
                $"Fact: {fact.Fact} length: {fact.Length}",
                It.IsAny<CancellationToken>()),
            Times.Exactly(3));
    }

    [TestMethod]
    public async Task RunLoopAsync_UserTypesCountWithInvalidValue_WritesError()
    {
        _consoleServiceMock
            .SetupSequence(c => c.ReadLineAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync("count abc")
            .ReturnsAsync("exit");

        var sut = CreateSut();

        await sut.RunLoopAsync(CancellationToken.None);

        _consoleServiceMock.Verify(
            c => c.WriteError("Usage: count <positive number>"),
            Times.Once);

        _catFactApiServiceMock.Verify(
            s => s.GetCatFactAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [TestMethod]
    public async Task RunLoopAsync_UserTypesCountWithZero_WritesError()
    {
        _consoleServiceMock
            .SetupSequence(c => c.ReadLineAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync("count 0")
            .ReturnsAsync("exit");

        var sut = CreateSut();

        await sut.RunLoopAsync(CancellationToken.None);

        _consoleServiceMock.Verify(
            c => c.WriteError("Usage: count <positive number>"),
            Times.Once);

        _catFactApiServiceMock.Verify(
            s => s.GetCatFactAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [TestMethod]
    public async Task RunLoopAsync_UserTypesCountWithNegativeValue_WritesError()
    {
        _consoleServiceMock
            .SetupSequence(c => c.ReadLineAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync("count -1")
            .ReturnsAsync("exit");

        var sut = CreateSut();

        await sut.RunLoopAsync(CancellationToken.None);

        _consoleServiceMock.Verify(
            c => c.WriteError("Usage: count <positive number>"),
            Times.Once);

        _catFactApiServiceMock.Verify(
            s => s.GetCatFactAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [TestMethod]
    public async Task RunLoopAsync_UserTypesCountWithoutArgument_WritesError()
    {
        _consoleServiceMock
            .SetupSequence(c => c.ReadLineAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync("count")
            .ReturnsAsync("exit");

        var sut = CreateSut();

        await sut.RunLoopAsync(CancellationToken.None);

        _consoleServiceMock.Verify(
            c => c.WriteError("Usage: count <positive number>"),
            Times.Once);

        _catFactApiServiceMock.Verify(
            s => s.GetCatFactAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [TestMethod]
    public async Task RunLoopAsync_UserTypesCountWithTooManyArguments_WritesError()
    {
        _consoleServiceMock
            .SetupSequence(c => c.ReadLineAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync("count 3 extra")
            .ReturnsAsync("exit");

        var sut = CreateSut();

        await sut.RunLoopAsync(CancellationToken.None);

        _consoleServiceMock.Verify(
            c => c.WriteError("Usage: count <positive number>"),
            Times.Once);

        _catFactApiServiceMock.Verify(
            s => s.GetCatFactAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [TestMethod]
    public async Task RunLoopAsync_UserTypesUnknownCommand_WritesError()
    {
        _consoleServiceMock
            .SetupSequence(c => c.ReadLineAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync("foobar")
            .ReturnsAsync("exit");

        var sut = CreateSut();

        await sut.RunLoopAsync(CancellationToken.None);

        _consoleServiceMock.Verify(
            c => c.WriteError("Unknown command: foobar"),
            Times.Once);

        _catFactApiServiceMock.Verify(
            s => s.GetCatFactAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [TestMethod]
    public async Task RunLoopAsync_UserTypesHelp_WritesAvailableCommands()
    {
        _consoleServiceMock
            .SetupSequence(c => c.ReadLineAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync("help")
            .ReturnsAsync("exit");

        var sut = CreateSut();

        await sut.RunLoopAsync(CancellationToken.None);

        _consoleServiceMock.Verify(
            c => c.WriteLine(It.Is<string>(message =>
                message.StartsWith("Available commands:") &&
                message.Contains("ENTER") &&
                message.Contains("once") &&
                message.Contains("count <n>") &&
                message.Contains("help") &&
                message.Contains("exit"))),
            Times.Once);
    }

    [TestMethod]
    public async Task RunLoopAsync_CountOption_FetchesRequestedNumberOfFactsAndStops()
    {
        var fact = new CatFact
        {
            Fact = "Test fact",
            Length = 9
        };

        _catFactApiServiceMock
            .Setup(s => s.GetCatFactAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(fact);

        var sut = CreateSut(new CommandLineOptions(false, 3));

        await sut.RunLoopAsync(CancellationToken.None);

        _catFactApiServiceMock.Verify(
            s => s.GetCatFactAsync(It.IsAny<CancellationToken>()),
            Times.Exactly(3));

        _filesystemServiceMock.Verify(
            f => f.AppendLineAsync(
                $"Fact: {fact.Fact} length: {fact.Length}",
                It.IsAny<CancellationToken>()),
            Times.Exactly(3));

        _consoleServiceMock.Verify(
            c => c.ReadLineAsync(It.IsAny<CancellationToken>()),
            Times.Never);

        _hostApplicationLifetimeMock.Verify(
            h => h.StopApplication(),
            Times.Once);
    }

    [TestMethod]
    public async Task RunLoopAsync_CountOption_ApiThrows_ContinuesFetching()
    {
        var fact = new CatFact
        {
            Fact = "Test fact",
            Length = 9
        };

        _catFactApiServiceMock
            .SetupSequence(s => s.GetCatFactAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("boom"))
            .ReturnsAsync(fact)
            .ReturnsAsync(fact);

        var sut = CreateSut(new CommandLineOptions(false, 3));

        await sut.RunLoopAsync(CancellationToken.None);

        _catFactApiServiceMock.Verify(
            s => s.GetCatFactAsync(It.IsAny<CancellationToken>()),
            Times.Exactly(3));

        _consoleServiceMock.Verify(
            c => c.WriteError("boom"),
            Times.Once);

        _filesystemServiceMock.Verify(
            f => f.AppendLineAsync(
                $"Fact: {fact.Fact} length: {fact.Length}",
                It.IsAny<CancellationToken>()),
            Times.Exactly(2));

        _hostApplicationLifetimeMock.Verify(
            h => h.StopApplication(),
            Times.Once);
    }

    [TestMethod]
    public async Task RunLoopAsync_ApiThrows_WritesErrorAndContinuesLoop()
    {
        _consoleServiceMock
            .SetupSequence(c => c.ReadLineAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(string.Empty)
            .ReturnsAsync("exit");

        _catFactApiServiceMock
            .Setup(s => s.GetCatFactAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("boom"));

        var sut = CreateSut();

        await sut.RunLoopAsync(CancellationToken.None);

        _consoleServiceMock.Verify(
            c => c.WriteError("boom"),
            Times.Once);

        _consoleServiceMock.Verify(
            c => c.ReadLineAsync(It.IsAny<CancellationToken>()),
            Times.Exactly(2));

        _hostApplicationLifetimeMock.Verify(
            h => h.StopApplication(),
            Times.Once);
    }

    [TestMethod]
    public async Task RunLoopAsync_FilesystemThrows_WritesErrorAndContinuesLoop()
    {
        var fact = new CatFact
        {
            Fact = "Test fact",
            Length = 9
        };

        _consoleServiceMock
            .SetupSequence(c => c.ReadLineAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(string.Empty)
            .ReturnsAsync("exit");

        _catFactApiServiceMock
            .Setup(s => s.GetCatFactAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(fact);

        _filesystemServiceMock
            .Setup(f => f.AppendLineAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new IOException("disk full"));

        var sut = CreateSut();

        await sut.RunLoopAsync(CancellationToken.None);

        _consoleServiceMock.Verify(
            c => c.WriteError("disk full"),
            Times.Once);

        _hostApplicationLifetimeMock.Verify(
            h => h.StopApplication(),
            Times.Once);
    }

    [TestMethod]
    public async Task RunLoopAsync_CancellationRequestedBeforeStart_DoesNotEnterLoop()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var sut = CreateSut();

        await sut.RunLoopAsync(cts.Token);

        _consoleServiceMock.Verify(
            c => c.ReadLineAsync(It.IsAny<CancellationToken>()),
            Times.Never);

        _hostApplicationLifetimeMock.Verify(
            h => h.StopApplication(),
            Times.Once);
    }

    [TestMethod]
    public async Task RunLoopAsync_ApiCallIsCanceled_StopsLoopGracefully()
    {
        _consoleServiceMock
            .Setup(c => c.ReadLineAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(string.Empty);

        _catFactApiServiceMock
            .Setup(s => s.GetCatFactAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

        var sut = CreateSut();

        await sut.RunLoopAsync(CancellationToken.None);

        _hostApplicationLifetimeMock.Verify(
            h => h.StopApplication(),
            Times.Once);

        _consoleServiceMock.Verify(
            c => c.ReadLineAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
