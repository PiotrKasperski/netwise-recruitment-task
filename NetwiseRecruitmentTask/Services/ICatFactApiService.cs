using NetwiseRecruitmentTask.Models;

namespace NetwiseRecruitmentTask.Services;

public interface ICatFactApiService
{
    Task<CatFact> GetCatFactAsync(CancellationToken cancellationToken = default);
}
