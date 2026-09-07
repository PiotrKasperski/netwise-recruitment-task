using System.Net.Http.Json;

public sealed class CatFactApiService : ICatFactApiService
{
    private readonly HttpClient _httpClient;

    public CatFactApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<CatFact> GetCatFactAsync(CancellationToken cancellationToken = default)
    {
        var fact = await _httpClient.GetFromJsonAsync<CatFact>("fact", cancellationToken);

        if (fact is null)
        {
            throw new InvalidOperationException("Can not read answer from catfact.ninja");
        }
        return fact;
    }
}
