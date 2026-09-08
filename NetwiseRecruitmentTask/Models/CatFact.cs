using System.Text.Json.Serialization;

namespace NetwiseRecruitmentTask.Models;

public sealed class CatFact
{
    [JsonPropertyName("fact")]
    public string Fact { get; set; } = string.Empty;
    [JsonPropertyName("length")]
    public int Length { get; set; }
}
