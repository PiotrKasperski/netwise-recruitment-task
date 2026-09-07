using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

public static class ServiceConfiguration
{
    public static void Configure(IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<CatFactSettings>()
            .Bind(configuration.GetSection(CatFactSettings.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddHostedService<Worker>();

        services.AddHttpClient<ICatFactApiService, CatFactApiService>((serviceProvider, client) =>
        {
            CatFactSettings settings = serviceProvider
                .GetRequiredService<IOptions<CatFactSettings>>().Value;

            client.BaseAddress = new Uri(settings.BaseAddress);
            client.Timeout = TimeSpan.FromSeconds(settings.RequestTimeoutSeconds);
        });

        services.AddSingleton<IFilesystemService, FilesystemService>();
    }
}
