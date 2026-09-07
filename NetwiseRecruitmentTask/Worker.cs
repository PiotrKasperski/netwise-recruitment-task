using Microsoft.Extensions.Hosting;

public sealed class Worker(ICatFactApiService _catFactApiService) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var fact = await _catFactApiService.GetCatFactAsync();
        Console.WriteLine(fact.Fact);
    }
}
