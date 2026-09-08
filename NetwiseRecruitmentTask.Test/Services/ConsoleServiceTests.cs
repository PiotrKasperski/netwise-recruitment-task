using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NetwiseRecruitmentTask.Services;

namespace NetwiseRecruitmentTask.Test.Services;

[TestClass]
[DoNotParallelize]
public sealed class ConsoleServiceTests
{
    private TextWriter _originalOut = null!;
    private TextWriter _originalError = null!;
    private ConsoleService _sut = null!;

    [TestInitialize]
    public void TestInitialize()
    {
        _originalOut = Console.Out;
        _originalError = Console.Error;
        _sut = new ConsoleService();
    }

    [TestCleanup]
    public void TestCleanup()
    {
        Console.SetOut(_originalOut);
        Console.SetError(_originalError);
    }

    [TestMethod]
    public void WriteLine_WritesMessageWithNewLine_ToStandardOutput()
    {
        using var sw = new StringWriter();
        Console.SetOut(sw);

        _sut.WriteLine("hello world");

        Assert.AreEqual("hello world" + Environment.NewLine, sw.ToString());
    }

    [TestMethod]
    public void Write_WritesMessage_WithoutNewLine_ToStandardOutput()
    {
        using var sw = new StringWriter();
        Console.SetOut(sw);

        _sut.Write("no newline here");

        Assert.AreEqual("no newline here", sw.ToString());
    }

    [TestMethod]
    public void WriteError_WritesMessageWithNewLine_ToErrorOutput()
    {
        using var sw = new StringWriter();
        Console.SetError(sw);

        _sut.WriteError("something failed");

        Assert.AreEqual("something failed" + Environment.NewLine, sw.ToString());
    }

    [TestMethod]
    public void WriteLine_DoesNotWriteToErrorStream()
    {
        using var outWriter = new StringWriter();
        using var errWriter = new StringWriter();
        Console.SetOut(outWriter);
        Console.SetError(errWriter);

        _sut.WriteLine("only stdout");

        Assert.AreEqual(string.Empty, errWriter.ToString());
    }

    [TestMethod]
    public void Write_DoesNotWriteToErrorStream()
    {
        using var outWriter = new StringWriter();
        using var errWriter = new StringWriter();
        Console.SetOut(outWriter);
        Console.SetError(errWriter);

        _sut.Write("only stdout");

        Assert.AreEqual(string.Empty, errWriter.ToString());
    }

    [TestMethod]
    public void WriteError_DoesNotWriteToStandardOutput()
    {
        using var outWriter = new StringWriter();
        using var errWriter = new StringWriter();
        Console.SetOut(outWriter);
        Console.SetError(errWriter);

        _sut.WriteError("boom");

        Assert.AreEqual(string.Empty, outWriter.ToString());
    }

    [TestMethod]
    public void Clear_DoesNotThrow_WhenConsoleUnavailable()
    {
        try
        {
            _sut.Clear();
        }
        catch (Exception ex)
        {
            Assert.Fail($"Clear() should not throw, but threw: {ex}");
        }
    }

    [TestMethod]
    public async Task ReadLineAsync_ThrowsOperationCanceledException_WhenTokenAlreadyCancelled()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await AssertThrowsAsync<OperationCanceledException>(
            () => _sut.ReadLineAsync(cts.Token));
    }

    [TestMethod]
    public async Task ReadLineAsync_ThrowsOperationCanceledException_WhenCancelledWhileWaitingForInput()
    {
        using var cts = new CancellationTokenSource();
        var readTask = _sut.ReadLineAsync(cts.Token);

        await Task.Delay(150);

        cts.Cancel();

        await AssertThrowsAsync<OperationCanceledException>(() => readTask);
    }

    private static async Task AssertThrowsAsync<TException>(Func<Task> action)
        where TException : Exception
    {
        try
        {
            await action();
        }
        catch (TException)
        {
            return;
        }
        catch (Exception ex)
        {
            Assert.Fail($"Expected exception {typeof(TException).Name}, but got {ex.GetType().Name}: {ex.Message}");
            return;
        }

        Assert.Fail($"Expected exception {typeof(TException).Name}, but none was thrown.");
    }
}
