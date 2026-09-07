public sealed class CatFactSettings
{
    public const string SectionName = "CatFactSettings";

    public string BaseAddress { get; set; } = string.Empty;
    public string FactEndpoint { get; set; } = string.Empty;
    public int RequestTimeoutSeconds { get; set; } = 10;
    public string OutputFileName { get; set; } = "cat_facts.txt";
}
