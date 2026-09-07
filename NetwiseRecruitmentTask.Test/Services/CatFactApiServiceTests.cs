using System.Net;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class CatFactApiServiceTests
{
    [TestMethod]
    public async Task GetCatFactAsync_ApiReturnsCatFact_ReturnsCatFact()
    {
        using var httpClient = CreateHttpClient(
            HttpStatusCode.OK,
            """{"fact":"Cats sleep for most of their lives.","length":38}"""
        );
        var sut = new CatFactApiService(httpClient, CreateOptions());

        var result = await sut.GetCatFactAsync();

        Assert.IsNotNull(result);
        Assert.AreEqual("Cats sleep for most of their lives.", result.Fact);
        Assert.AreEqual(38, result.Length);
    }

    [TestMethod]
    public async Task GetCatFactAsync_ApiReturnsNull_ThrowsInvalidOperationException()
    {
        using var httpClient = CreateHttpClient(
            HttpStatusCode.OK,
            "null"
        );
        var sut = new CatFactApiService(httpClient, CreateOptions());

        try
        {
            await sut.GetCatFactAsync();
            Assert.Fail("Expected InvalidOperationException.");
        }
        catch (InvalidOperationException ex)
        {
            Assert.AreEqual(
                "Can not read answer from catfact.ninja",
                ex.Message
            );
        }
    }

    [TestMethod]
    public async Task GetCatFactAsync_RequestIsCanceled_ThrowsOperationCanceledException()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        using var httpClient = CreateHttpClient(
            HttpStatusCode.OK,
            """{"fact":"Test fact","length":9}"""
        );
        var sut = new CatFactApiService(httpClient, CreateOptions());

        try
        {
            await sut.GetCatFactAsync(cts.Token);
            Assert.Fail("Expected OperationCanceledException.");
        }
        catch (OperationCanceledException)
        {
        }
    }

    [TestMethod]
    public async Task GetCatFactAsync_ApiReturnsErrorStatus_ThrowsHttpRequestException()
    {
        using var httpClient = CreateHttpClient(
            HttpStatusCode.InternalServerError,
            string.Empty
        );
        var sut = new CatFactApiService(httpClient, CreateOptions());

        try
        {
            await sut.GetCatFactAsync();
            Assert.Fail("Expected HttpRequestException.");
        }
        catch (HttpRequestException)
        {
        }
    }

    private static IOptions<CatFactSettings> CreateOptions() =>
        Options.Create(new CatFactSettings
        {
            BaseAddress = "https://catfact.ninja/",
            FactEndpoint = "fact",
            RequestTimeoutSeconds = 10,
            OutputFileName = "cat_facts.txt"
        });

    private static HttpClient CreateHttpClient(
        HttpStatusCode statusCode,
        string content)
    {
        var handler = new TestHttpMessageHandler(statusCode, content);
        return new HttpClient(handler)
        {
            BaseAddress = new Uri("https://catfact.ninja/")
        };
    }

    private sealed class TestHttpMessageHandler : HttpMessageHandler
    {
        private readonly HttpStatusCode _statusCode;
        private readonly string _content;

        public TestHttpMessageHandler(
            HttpStatusCode statusCode,
            string content)
        {
            _statusCode = statusCode;
            _content = content;
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var response = new HttpResponseMessage(_statusCode)
            {
                Content = new StringContent(
                    _content,
                    Encoding.UTF8,
                    "application/json")
            };
            return Task.FromResult(response);
        }
    }
}
