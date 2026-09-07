using System.Text.Json.Serialization;

public sealed class CatFact
{
    [JsonPropertyName("fact")]
    public string Fact { get; set; } = String.Empty;
    [JsonPropertyName("length")]
    public int Length { get; set; }
}
