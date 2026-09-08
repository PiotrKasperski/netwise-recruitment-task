using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NetwiseRecruitmentTask;
using NetwiseRecruitmentTask.Services;

[TestClass]
public class ServiceConfigurationTests
{
    private IServiceProvider _provider = null!;

    [TestInitialize]
    public void Setup()
    {
        var builder = Host.CreateApplicationBuilder();

        var inMemorySettings = new Dictionary<string, string?>
        {
            ["CatFactSettings:BaseAddress"] = "https://catfact.ninja/",
            ["CatFactSettings:FactEndpoint"] = "fact",
            ["CatFactSettings:RequestTimeoutSeconds"] = "10",
            ["CatFactSettings:OutputFileName"] = "cat_facts.txt"
        };

        builder.Configuration.AddInMemoryCollection(inMemorySettings);

        var options = new CommandLineOptions(false, null);

        ServiceConfiguration.Configure(
            builder.Services,
            builder.Configuration,
            options);

        var host = builder.Build();
        _provider = host.Services;
    }

    [TestMethod]
    public void Configure_RegistersWorkerAsHostedService()
    {
        var hostedServices = _provider.GetServices<IHostedService>();

        Assert.IsTrue(hostedServices.Any(s => s is Worker));
    }

    [TestMethod]
    public void Configure_RegistersCatFactApiServiceWithCorrectBaseAddress()
    {
        var factory = _provider.GetRequiredService<IHttpClientFactory>();
        var client = factory.CreateClient(nameof(ICatFactApiService));

        Assert.IsNotNull(client.BaseAddress);
        Assert.AreEqual(
            new Uri("https://catfact.ninja/"),
            client.BaseAddress);
    }

    [TestMethod]
    public void Configure_RegistersCatFactApiServiceWithCorrectTimeout()
    {
        var factory = _provider.GetRequiredService<IHttpClientFactory>();
        var client = factory.CreateClient(nameof(ICatFactApiService));

        Assert.AreEqual(
            TimeSpan.FromSeconds(10),
            client.Timeout);
    }

    [TestMethod]
    public void Configure_RegistersICatFactApiServiceAsResolvable()
    {
        var service = _provider.GetService<ICatFactApiService>();

        Assert.IsNotNull(service);
        Assert.IsInstanceOfType(
            service,
            typeof(CatFactApiService));
    }

    [TestMethod]
    public void Configure_RegistersIFilesystemServiceAsSingleton()
    {
        var instance1 = _provider.GetRequiredService<IFilesystemService>();
        var instance2 = _provider.GetRequiredService<IFilesystemService>();

        Assert.AreSame(instance1, instance2);
        Assert.IsInstanceOfType(
            instance1,
            typeof(FilesystemService));
    }

    [TestMethod]
    public void Configure_RegistersCommandLineOptions()
    {
        var options = _provider.GetRequiredService<CommandLineOptions>();

        Assert.IsFalse(options.Once);
        Assert.IsNull(options.Count);
    }
}
