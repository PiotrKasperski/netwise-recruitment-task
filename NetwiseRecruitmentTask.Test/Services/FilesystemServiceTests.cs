using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NetwiseRecruitmentTask.Services;

[TestClass]
[DoNotParallelize]
public class FilesystemServiceTests
{
    private FilesystemService _sut = null!;

    [TestInitialize]
    public void Setup()
    {
        var settings = new CatFactSettings
        {
            OutputFileName = "cat_facts.txt"
        };

        _sut = new FilesystemService(Options.Create(settings));

        if (File.Exists(_sut.FilePath))
        {
            File.Delete(_sut.FilePath);
        }
    }

    [TestCleanup]
    public void Cleanup()
    {
        if (File.Exists(_sut.FilePath))
        {
            File.Delete(_sut.FilePath);
        }
    }

    [TestMethod]
    public void FilePath_PointsToFileInApplicationDirectory()
    {
        var expected = Path.Combine(AppContext.BaseDirectory, "cat_facts.txt");
        Assert.AreEqual(expected, _sut.FilePath);
    }

    [TestMethod]
    public void EnsureFileExist_FileDoesNotExist_CreatesFile()
    {
        Assert.IsFalse(File.Exists(_sut.FilePath));
        _sut.EnsureFileExist();
        Assert.IsTrue(File.Exists(_sut.FilePath));
    }

    [TestMethod]
    public void EnsureFileExist_FileAlreadyExists_DoesNotOverwriteContent()
    {
        File.WriteAllText(_sut.FilePath, "existing content");
        _sut.EnsureFileExist();
        var content = File.ReadAllText(_sut.FilePath);
        Assert.AreEqual("existing content", content);
    }

    [TestMethod]
    public void EnsureFileExist_CalledMultipleTimes_DoesNotThrowException()
    {
        _sut.EnsureFileExist();
        _sut.EnsureFileExist();
        Assert.IsTrue(File.Exists(_sut.FilePath));
    }

    [TestMethod]
    public async Task AppendLineAsync_FileDoesNotExist_CreatesFileAndWritesLine()
    {
        await _sut.AppendLineAsync("first line");
        Assert.IsTrue(File.Exists(_sut.FilePath));
        var content = await File.ReadAllTextAsync(_sut.FilePath);
        Assert.AreEqual("first line" + Environment.NewLine, content);
    }

    [TestMethod]
    public async Task AppendLineAsync_CalledMultipleTimes_AppendsSubsequentLines()
    {
        await _sut.AppendLineAsync("line 1");
        await _sut.AppendLineAsync("line 2");
        await _sut.AppendLineAsync("line 3");
        var lines = await File.ReadAllLinesAsync(_sut.FilePath);
        CollectionAssert.AreEqual(new[] { "line 1", "line 2", "line 3" }, lines);
    }

    [TestMethod]
    public async Task AppendLineAsync_EmptyString_WritesOnlyNewLine()
    {
        await _sut.AppendLineAsync(string.Empty);
        var content = await File.ReadAllTextAsync(_sut.FilePath);
        Assert.AreEqual(Environment.NewLine, content);
    }

    [TestMethod]
    public async Task AppendLineAsync_CanceledToken_ThrowsOperationCanceledException()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        try
        {
            await _sut.AppendLineAsync("line", cts.Token);
            Assert.Fail("Expected OperationCanceledException.");
        }
        catch (OperationCanceledException)
        {
        }
    }
}
