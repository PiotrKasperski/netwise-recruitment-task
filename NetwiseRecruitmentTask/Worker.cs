using Microsoft.Extensions.Hosting;

public sealed class Worker(IHostApplicationLifetime hostApplicationLifetime, ICatFactApiService _catFactApiService, IFilesystemService _filesystemService) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Console.Clear();
        _filesystemService.EnsureFileExist();
        while (true)
        {
            Console.WriteLine("Push ENTER to get new fact or type 'exit' to close the app");

            Console.Write("> ");
            var input = Console.ReadLine();

            if (String.Equals(input, "exit", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine("Exited");
                break;
            }

            try
            {
                var fact = await _catFactApiService.GetCatFactAsync();
                Console.Clear();
                await _filesystemService.AppendLineAsync($"Fact: {fact.Fact} length: {fact.Length}");
                Console.WriteLine(fact.Fact);
            }
            catch (Exception ex)
            {
                TextWriter errorWriter = Console.Error;
                errorWriter.WriteLine(ex.Message);
            }
        }
        hostApplicationLifetime.StopApplication();

    }
}
