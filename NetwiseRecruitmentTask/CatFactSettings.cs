using System.ComponentModel.DataAnnotations;

public sealed class CatFactSettings
{
    public const string SectionName = "CatFactSettings";

    [Required]
    [Url]
    public string BaseAddress { get; set; } = string.Empty;

    [Required]
    public string FactEndpoint { get; set; } = string.Empty;

    [Range(1, 300)]
    public int RequestTimeoutSeconds { get; set; } = 10;

    [Required]
    public string OutputFileName { get; set; } = "cat_facts.txt";
}
