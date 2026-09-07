public interface IConsoleService
{
    string? ReadLine();
    void WriteLine(string message);
    void Write(string message);
    void WriteError(string message);
    void Clear();
}
