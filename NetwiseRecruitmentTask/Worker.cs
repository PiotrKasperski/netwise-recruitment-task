using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NetwiseRecruitmentTask.Services;

namespace NetwiseRecruitmentTask;

public sealed class Worker(
    IHostApplicationLifetime hostApplicationLifetime,
    ICatFactApiService catFactApiService,
    IFilesystemService filesystemService,
    IConsoleService console,
    ILogger<Worker> logger) : BackgroundService
{
    protected override Task ExecuteAsync(CancellationToken stoppingToken) =>
        RunLoopAsync(stoppingToken);

    internal async Task RunLoopAsync(CancellationToken stoppingToken)
    {
        console.Clear();
        filesystemService.EnsureFileExist();


        while (!stoppingToken.IsCancellationRequested)
        {
            console.WriteLine("Push ENTER to get new fact or type 'exit' to close the app");
            console.Write("> ");

            var input = await console.ReadLineAsync(stoppingToken);

            if (string.Equals(input, "exit", StringComparison.OrdinalIgnoreCase))
            {
                console.WriteLine("Exited");
                break;
            }

            try
            {
                var fact = await catFactApiService.GetCatFactAsync(stoppingToken);
                await filesystemService.AppendLineAsync($"Fact: {fact.Fact} length: {fact.Length}", stoppingToken);

                console.Clear();
                console.WriteLine(fact.Fact);

            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to fetch or save cat fact");
                console.WriteError(ex.Message);
            }
        }

        hostApplicationLifetime.StopApplication();
    }
}
