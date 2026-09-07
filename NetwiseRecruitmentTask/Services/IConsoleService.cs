namespace NetwiseRecruitmentTask.Services;

public interface IConsoleService
{
    Task<string?> ReadLineAsync(CancellationToken cancellationToken);
    void WriteLine(string message);
    void Write(string message);
    void WriteError(string message);
    void Clear();
}
