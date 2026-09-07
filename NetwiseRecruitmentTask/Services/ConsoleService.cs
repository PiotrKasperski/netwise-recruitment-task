public sealed class ConsoleService : IConsoleService
{
    public string? ReadLine() => Console.ReadLine();

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
}
