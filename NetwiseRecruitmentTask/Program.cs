using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

builder.Services.AddHttpClient<ICatFactApiService, CatFactApiService>(client =>
{
    client.BaseAddress = new Uri("https://catfact.ninja/");
    client.Timeout = TimeSpan.FromSeconds(10);
});
