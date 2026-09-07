using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using NetwiseRecruitmentTask.Models;

namespace NetwiseRecruitmentTask.Services;

public sealed class CatFactApiService : ICatFactApiService
{
    private readonly HttpClient _httpClient;
    private readonly CatFactSettings _settings;

    public CatFactApiService(HttpClient httpClient, IOptions<CatFactSettings> options)
    {
        _httpClient = httpClient;
        _settings = options.Value;
    }

    public async Task<CatFact> GetCatFactAsync(CancellationToken cancellationToken = default)
    {
        var fact = await _httpClient.GetFromJsonAsync<CatFact>(_settings.FactEndpoint, cancellationToken);

        if (fact is null)
        {
            throw new InvalidOperationException("Can not read answer from catfact.ninja");
        }
        return fact;
    }
}
