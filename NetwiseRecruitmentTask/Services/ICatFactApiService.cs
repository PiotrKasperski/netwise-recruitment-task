public interface ICatFactApiService
{
    Task<CatFact> GetCatFactAsync(CancellationToken cancellationToken = default);
}
