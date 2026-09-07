using Microsoft.Extensions.Options;

public sealed class FilesystemService : IFilesystemService
{
    public string FilePath { get; }

    public FilesystemService(IOptions<CatFactSettings> options)
    {
        FilePath = Path.Combine(AppContext.BaseDirectory, options.Value.OutputFileName);
    }


    public async Task AppendLineAsync(string line, CancellationToken cancellationToken = default)
    {
        await File.AppendAllTextAsync(FilePath, line + Environment.NewLine, cancellationToken);

    }

    public void EnsureFileExist()
    {
        if (!File.Exists(FilePath))
        {
            using var stream = File.Create(FilePath);
        }
    }
}
