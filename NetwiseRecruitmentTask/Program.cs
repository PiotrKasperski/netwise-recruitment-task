using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
builder.Logging.AddFilter("Microsoft.Hosting.Lifetime", LogLevel.None);

builder.Services.AddHostedService<Worker>();

builder.Services.AddHttpClient<ICatFactApiService, CatFactApiService>(client =>
{
    client.BaseAddress = new Uri("https://catfact.ninja/");
    client.Timeout = TimeSpan.FromSeconds(10);
});

builder.Services.AddSingleton<IFilesystemService, FilesystemService>();

IHost host = builder.Build();

host.Run();
