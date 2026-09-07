using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

[TestClass]
[DoNotParallelize]
public class WorkerTests
{
    private Mock<IHostApplicationLifetime> _lifetimeMock = null!;
    private Mock<ICatFactApiService> _catFactApiServiceMock = null!;
    private Mock<IFilesystemService> _filesystemServiceMock = null!;

    private TextReader _originalIn = null!;
    private TextWriter _originalOut = null!;
    private TextWriter _originalError = null!;

    [TestInitialize]
    public void Setup()
    {
        _lifetimeMock = new Mock<IHostApplicationLifetime>();
        _catFactApiServiceMock = new Mock<ICatFactApiService>();
        _filesystemServiceMock = new Mock<IFilesystemService>();

        _originalIn = Console.In;
        _originalOut = Console.Out;
        _originalError = Console.Error;
    }

    [TestCleanup]
    public void TearDown()
    {
        Console.SetIn(_originalIn);
        Console.SetOut(_originalOut);
        Console.SetError(_originalError);
    }

    private Worker CreateWorker()
        => new Worker(_lifetimeMock.Object, _catFactApiServiceMock.Object, _filesystemServiceMock.Object);

    private static Task RunWorkerAsync(Worker worker, CancellationToken token)
    {
        var executeAsyncMethod = typeof(Worker).GetMethod(
            "ExecuteAsync",
            BindingFlags.NonPublic | BindingFlags.Instance);

        if (executeAsyncMethod is null)
        {
            throw new InvalidOperationException(
                "Could not find the ExecuteAsync method on the Worker type. Check the method name/signature.");
        }

        return (Task)executeAsyncMethod.Invoke(worker, new object[] { token })!;
    }

    private static void SetConsoleInput(params string[] lines)
    {
        var input = string.Join(Environment.NewLine, lines) + Environment.NewLine;
        Console.SetIn(new StringReader(input));
    }

    [TestMethod]
    public async Task ExecuteAsync_Should_EnsureFileExists_OnStartup()
    {
        SetConsoleInput("exit");
        var worker = CreateWorker();

        await RunWorkerAsync(worker, CancellationToken.None);

        _filesystemServiceMock.Verify(x => x.EnsureFileExist(), Times.Once);
    }

    [TestMethod]
    public async Task ExecuteAsync_Should_StopApplication_When_UserTypesExit()
    {
        SetConsoleInput("exit");
        var worker = CreateWorker();

        await RunWorkerAsync(worker, CancellationToken.None);

        _lifetimeMock.Verify(x => x.StopApplication(), Times.Once);
        _catFactApiServiceMock.Verify(x => x.GetCatFactAsync(), Times.Never);
    }

    [TestMethod]
    [DataRow("EXIT")]
    [DataRow("Exit")]
    [DataRow("eXiT")]
    public async Task ExecuteAsync_Should_Exit_CaseInsensitively(string exitCommand)
    {
        SetConsoleInput(exitCommand);
        var worker = CreateWorker();

        await RunWorkerAsync(worker, CancellationToken.None);

        _lifetimeMock.Verify(x => x.StopApplication(), Times.Once);
    }

    [TestMethod]
    public async Task ExecuteAsync_Should_FetchFactAndAppendToFile_When_UserPressesEnter()
    {
        SetConsoleInput("", "exit");

        var fact = new CatFact { Fact = "Cats sleep a lot.", Length = 17 };
        _catFactApiServiceMock
            .Setup(x => x.GetCatFactAsync())
            .ReturnsAsync(fact);

        var worker = CreateWorker();

        await RunWorkerAsync(worker, CancellationToken.None);

        _catFactApiServiceMock.Verify(x => x.GetCatFactAsync(), Times.Once);
        _filesystemServiceMock.Verify(
            x => x.AppendLineAsync($"Fact: {fact.Fact} length: {fact.Length}"),
            Times.Once);
    }

    [TestMethod]
    public async Task ExecuteAsync_Should_PrintFact_ToConsoleOutput()
    {
        SetConsoleInput("", "exit");
        var outputWriter = new StringWriter();
        Console.SetOut(outputWriter);

        var fact = new CatFact { Fact = "Cats have 32 muscles in each ear.", Length = 34 };
        _catFactApiServiceMock
            .Setup(x => x.GetCatFactAsync())
            .ReturnsAsync(fact);

        var worker = CreateWorker();

        await RunWorkerAsync(worker, CancellationToken.None);

        StringAssert.Contains(outputWriter.ToString(), fact.Fact);
    }

    [TestMethod]
    public async Task ExecuteAsync_Should_LoopMultipleTimes_UntilExit()
    {
        SetConsoleInput("", "", "exit");

        _catFactApiServiceMock
            .Setup(x => x.GetCatFactAsync())
            .ReturnsAsync(new CatFact { Fact = "Some fact", Length = 9 });

        var worker = CreateWorker();

        await RunWorkerAsync(worker, CancellationToken.None);

        _catFactApiServiceMock.Verify(x => x.GetCatFactAsync(), Times.Exactly(2));
        _filesystemServiceMock.Verify(
            x => x.AppendLineAsync(It.IsAny<string>()),
            Times.Exactly(2));
    }

    [TestMethod]
    public async Task ExecuteAsync_Should_WriteErrorToConsoleError_When_ApiThrows()
    {
        SetConsoleInput("", "exit");
        var errorWriter = new StringWriter();
        Console.SetError(errorWriter);

        _catFactApiServiceMock
            .Setup(x => x.GetCatFactAsync())
            .ThrowsAsync(new InvalidOperationException("API is down"));

        var worker = CreateWorker();

        await RunWorkerAsync(worker, CancellationToken.None);

        StringAssert.Contains(errorWriter.ToString(), "API is down");
        _filesystemServiceMock.Verify(
            x => x.AppendLineAsync(It.IsAny<string>()),
            Times.Never);
    }

    [TestMethod]
    public async Task ExecuteAsync_Should_ContinueLoop_After_ExceptionIsThrown()
    {
        SetConsoleInput("", "", "exit");
        Console.SetError(new StringWriter());

        var fact = new CatFact { Fact = "Recovered fact", Length = 14 };
        _catFactApiServiceMock
            .SetupSequence(x => x.GetCatFactAsync())
            .ThrowsAsync(new InvalidOperationException("boom"))
            .ReturnsAsync(fact);

        var worker = CreateWorker();

        await RunWorkerAsync(worker, CancellationToken.None);

        _catFactApiServiceMock.Verify(x => x.GetCatFactAsync(), Times.Exactly(2));
        _filesystemServiceMock.Verify(
            x => x.AppendLineAsync($"Fact: {fact.Fact} length: {fact.Length}"),
            Times.Once);
        _lifetimeMock.Verify(x => x.StopApplication(), Times.Once);
    }
}
