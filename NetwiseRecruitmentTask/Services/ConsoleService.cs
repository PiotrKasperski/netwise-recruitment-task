using System.Text;

namespace NetwiseRecruitmentTask.Services;

public sealed class ConsoleService : IConsoleService
{

    public void WriteLine(string message) => Console.WriteLine(message);

    public void Write(string message) => Console.Write(message);

    public void WriteError(string message) => Console.Error.WriteLine(message);

    public void Clear()
    {
        try
        {
            Console.Clear();
        }
        catch (IOException)
        {
            // No console available (e.g. redirected output, running as a service) — ignore.
        }
    }

    public async Task<string?> ReadLineAsync(CancellationToken cancellationToken)
    {
        var input = new StringBuilder();

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!Console.KeyAvailable)
            {
                await Task.Delay(50, cancellationToken);
                continue;
            }

            var key = Console.ReadKey(intercept: true);

            if (key.Key == ConsoleKey.Enter)
            {
                Console.WriteLine();
                return input.ToString();
            }

            if (key.Key == ConsoleKey.Backspace)
            {
                if (input.Length == 0)
                    continue;

                input.Length--;
                Console.Write("\b \b");
                continue;
            }

            if (!char.IsControl(key.KeyChar))
            {
                input.Append(key.KeyChar);
                Console.Write(key.KeyChar);
            }
        }
    }
}
