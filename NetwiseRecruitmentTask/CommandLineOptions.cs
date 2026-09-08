namespace NetwiseRecruitmentTask;

public sealed record CommandLineOptions(bool Once, int? Count)
{
    public static CommandLineOptions Parse(string[] args)
    {
        if (args.Length == 0)
            return new(false, null);

        if (args.Length == 1 && string.Equals(args[0], "--once", StringComparison.OrdinalIgnoreCase))
            return new(true, 1);

        if (args.Length == 2 &&
            string.Equals(args[0], "--count", StringComparison.OrdinalIgnoreCase) &&
            int.TryParse(args[1], out var count) &&
            count > 0)
        {
            return new(false, count);
        }

        throw new ArgumentException(
            "Usage: --once or --count <positive number>");
    }
}
