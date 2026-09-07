using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

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

    private Worker CreateSut() => new(
        _hostApplicationLifetimeMock.Object,
        _catFactApiServiceMock.Object,
        _filesystemServiceMock.Object,
        _consoleServiceMock.Object,
        _loggerMock.Object);

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
