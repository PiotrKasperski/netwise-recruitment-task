using Microsoft.Extensions.DependencyInjection;

public static class ServiceConfiguration
{
    public static void Configure(IServiceCollection services)
    {
        services.AddHostedService<Worker>();
        services.AddHttpClient<ICatFactApiService, CatFactApiService>(client =>
        {
            client.BaseAddress = new Uri("https://catfact.ninja/");
            client.Timeout = TimeSpan.FromSeconds(10);
        });
        services.AddSingleton<IFilesystemService, FilesystemService>();
    }
}
