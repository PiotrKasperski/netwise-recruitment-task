public sealed class FilesystemService : IFilesystemService
{
    public string FilePath { get; }
    public FilesystemService()
    {
        FilePath = Path.Combine(AppContext.BaseDirectory, "cat_facts.txt");
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
