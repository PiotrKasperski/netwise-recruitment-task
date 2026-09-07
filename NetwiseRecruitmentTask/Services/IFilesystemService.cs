namespace NetwiseRecruitmentTask.Services;

public interface IFilesystemService
{
    void EnsureFileExist();
    Task AppendLineAsync(string line, CancellationToken cancellationToken = default);

    string FilePath { get; }
}
