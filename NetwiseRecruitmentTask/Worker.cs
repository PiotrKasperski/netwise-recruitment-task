using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NetwiseRecruitmentTask.Services;

namespace NetwiseRecruitmentTask;

public sealed class Worker(
    IHostApplicationLifetime hostApplicationLifetime,
    ICatFactApiService catFactApiService,
    IFilesystemService filesystemService,
    IConsoleService console,
    ILogger<Worker> logger,
    CommandLineOptions options) : BackgroundService
{
    protected override Task ExecuteAsync(CancellationToken stoppingToken) =>
        RunLoopAsync(stoppingToken);

    internal async Task RunLoopAsync(CancellationToken stoppingToken)
    {
        console.Clear();
        filesystemService.EnsureFileExist();

        try
        {
            if (options.Count.HasValue)
            {
                await FetchFactsAsync(options.Count.Value, stoppingToken);
                return;
            }

            await RunInteractiveAsync(stoppingToken);
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation("Worker cancellation requested");
        }
        finally
        {
            hostApplicationLifetime.StopApplication();
        }
    }

    private async Task RunInteractiveAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            console.WriteLine(
                "ENTER: new fact | once | count <n> | help | exit");
            console.Write("> ");

            var input = await console.ReadLineAsync(stoppingToken);

            if (string.IsNullOrWhiteSpace(input))
            {
                await TryFetchFactAsync(stoppingToken);
                continue;
            }

            var parts = input.Trim().Split(
                ' ',
                StringSplitOptions.RemoveEmptyEntries);

            switch (parts[0].ToLowerInvariant())
            {
                case "exit":
                    console.WriteLine("Exited");
                    return;

                case "once":
                    await TryFetchFactAsync(stoppingToken);
                    break;

                case "count":
                    await HandleCountCommandAsync(parts, stoppingToken);
                    break;

                case "help":
                    WriteHelp();
                    break;

                default:
                    console.WriteError($"Unknown command: {parts[0]}");
                    break;
            }
        }
    }

    private async Task HandleCountCommandAsync(
        string[] parts,
        CancellationToken stoppingToken)
    {
        if (parts.Length != 2 ||
            !int.TryParse(parts[1], out var count) ||
            count <= 0)
        {
            console.WriteError("Usage: count <positive number>");
            return;
        }

        await FetchFactsAsync(count, stoppingToken);
    }

    private async Task FetchFactsAsync(
        int count,
        CancellationToken stoppingToken)
    {
        for (var i = 0; i < count; i++)
        {
            await TryFetchFactAsync(stoppingToken);
        }
    }

    private async Task TryFetchFactAsync(CancellationToken stoppingToken)
    {
        try
        {
            await FetchFactAsync(stoppingToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to fetch or save cat fact");
            console.WriteError(ex.Message);
        }
    }

    private async Task FetchFactAsync(CancellationToken stoppingToken)
    {
        var fact = await catFactApiService.GetCatFactAsync(stoppingToken);

        await filesystemService.AppendLineAsync(
            $"Fact: {fact.Fact} length: {fact.Length}",
            stoppingToken);

        console.Clear();
        console.WriteLine(fact.Fact);
    }

    private void WriteHelp()
    {
        console.WriteLine("""
            Available commands:
              ENTER       Get a new cat fact
              once        Get a single cat fact
              count <n>   Get n cat facts
              help        Show available commands
              exit        Close the application
            """);
    }
}
