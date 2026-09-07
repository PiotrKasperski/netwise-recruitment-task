using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
builder.Logging.AddFilter("Microsoft.Hosting.Lifetime", LogLevel.None);
ServiceConfiguration.Configure(builder.Services, builder.Configuration);

IHost host = builder.Build();

host.Run();

public partial class Program { }
