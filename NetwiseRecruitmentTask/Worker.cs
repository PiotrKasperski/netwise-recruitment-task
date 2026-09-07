using Microsoft.Extensions.Hosting;

public sealed class Worker(ICatFactApiService _catFactApiService, IFilesystemService _filesystemService) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _filesystemService.EnsureFileExist();
        var fact = await _catFactApiService.GetCatFactAsync();
        await _filesystemService.AppendLineAsync("Fact: " + fact.Fact + " length: " + fact.Length.ToString());
        Console.Clear();
        Console.WriteLine(fact.Fact);
    }
}
