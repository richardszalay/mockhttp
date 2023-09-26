using System.Net;
using System.Text;
using FluentAssertions;
using RichardSzalay.MockHttp.Extensions;

namespace RichardSzalay.MockHttp.Tests;

[TestClass]
public class MockedRequestExtentionsRespondTests
{
    private MockHttpMessageHandler _mockHandler = null!;

    private readonly HttpRequestMessage _request = new(HttpMethod.Post,
        "http://www.tempuri.org/path?apple=red&pear=green")
    {
        Content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "data1", "value1" },
            { "data2", "value2" }
        })
    };

    [TestInitialize]
    public void SetUp()
    {
        _mockHandler = new MockHttpMessageHandler();
    }

    [TestMethod]
    public async Task RespondHttpContent()
    {
        var mockedRequest = _mockHandler.When("/path");

        var response = await mockedRequest.Respond(new StringContent("test", Encoding.UTF8, "text/plain"))
            .SendAsync(_request, CancellationToken.None);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType!.MediaType.Should().Be("text/plain");
        (await response.Content.ReadAsStringAsync()).Should().Be("test");
    }

    [TestMethod]
    public async Task RespondHttpContentUnique()
    {
        var mockedRequest = _mockHandler.When("/path")
            .Respond(req => new StringContent("test", Encoding.UTF8, "text/plain"));

        var contentA = (await mockedRequest.SendAsync(_request, CancellationToken.None)).Content;
        var contentB = (await mockedRequest.SendAsync(_request, CancellationToken.None)).Content;

        contentA.Should().NotBeNull();
        contentB.Should().NotBeNull();
        contentA.Should().NotBeSameAs(contentB);
    }

    [TestMethod]
    public async Task RespondStringUnique()
    {
        var mockedRequest = _mockHandler.When("/path")
            .Respond("application/json", "{\"test\":\"value\"}");

        var contentA = (await mockedRequest.SendAsync(_request, CancellationToken.None)).Content;
        var contentB = (await mockedRequest.SendAsync(_request, CancellationToken.None)).Content;

        contentA.Should().NotBeNull();
        contentB.Should().NotBeNull();
        contentA.Should().NotBeSameAs(contentB);
    }

    [TestMethod]
    public async Task RespondStreamRereadIfPossible()
    {
        var mockedRequest = _mockHandler.When("/path")
            .Respond("text/plain", new MemoryStream(Encoding.UTF8.GetBytes("test")));

        var responseA = await mockedRequest.SendAsync(_request, CancellationToken.None);
        var responseB = await mockedRequest.SendAsync(_request, CancellationToken.None);

        responseA.Content.ReadAsStringAsync().Result.Should().NotBeSameAs(responseB.Content.ReadAsStringAsync().Result);
    }

    [TestMethod]
    public async Task RespondStreamHandler()
    {
        var mockedRequest = _mockHandler.When("/path")
            .Respond("text/plain", req => new MemoryStream(Encoding.UTF8.GetBytes("test")));

        var responseA = await mockedRequest.SendAsync(_request, CancellationToken.None);
        var responseB = await mockedRequest.SendAsync(_request, CancellationToken.None);

        responseA.Content.ReadAsStringAsync().Result.Should().NotBeSameAs(responseB.Content.ReadAsStringAsync().Result);
    }

    [TestMethod]
    public async Task RespondHttpMessage()
    {
        var expected = new HttpResponseMessage();
        var response = await _mockHandler.When("/path")
            .Respond(expected)
            .SendAsync(_request, CancellationToken.None);

        response.Should().BeSameAs(expected);
    }

    [TestMethod]
    public async Task RespondHttpStatusCode()
    {
        var response = await _mockHandler.When("/path")
            .Respond(HttpStatusCode.NoContent)
            .SendAsync(_request, CancellationToken.None);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [TestMethod]
    public async Task RespondFuncRequestResponse()
    {
        var expected = new HttpResponseMessage();
        var response = await _mockHandler.When("/path")
            .Respond(req => expected)
            .SendAsync(_request, CancellationToken.None);

        response.Should().BeSameAs(expected);
    }

    [TestMethod]
    public async Task RespondFuncRequestAsyncResponse()
    {
        var expected = new HttpResponseMessage();
        var response = await _mockHandler.When("/path")
            .Respond(req => Task.FromResult(expected))
            .SendAsync(_request, CancellationToken.None);

        response.Should().BeSameAs(expected);
    }

    [TestMethod]
    public async Task RespondHttpStatusHttpContent()
    {
        var response = await _mockHandler.When("/path")
            .Respond(HttpStatusCode.Found, new StringContent("test", Encoding.UTF8, "text/plain"))
            .SendAsync(_request, CancellationToken.None);

        response.StatusCode.Should().Be(HttpStatusCode.Found);
        response.Content.Headers.ContentType!.MediaType.Should().Be("text/plain");
        response.Content.ReadAsStringAsync().Result.Should().Be("test");
    }

    [TestMethod]
    public async Task RespondHttpStatusHeadersHttpContent()
    {
        var response = await _mockHandler.When("/path")
            .Respond(HttpStatusCode.Found, new Dictionary<string, string>
            {
                { "connection", "keep-alive" },
                { "x-hello", "mockhttp" }
            }, new StringContent("test", Encoding.UTF8, "text/plain"))
            .SendAsync(_request, CancellationToken.None);

        response.StatusCode.Should().Be(HttpStatusCode.Found);
        response.Content.Headers.ContentType!.MediaType.Should().Be("text/plain");
        response.Content.ReadAsStringAsync().Result.Should().Be("test");
        response.Headers.Connection.First().Should().Be("keep-alive");
        response.Headers.GetValues("x-hello").First().Should().Be("mockhttp");
    }

    [TestMethod]
    public async Task RespondMediaTypeStringContentStream()
    {
        var ms = new MemoryStream(Encoding.UTF8.GetBytes("test"));

        var response = await _mockHandler.When("/path")
            .Respond("text/plain", ms)
            .SendAsync(_request, CancellationToken.None);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType!.MediaType.Should().Be("text/plain");
        response.Content.ReadAsStringAsync().Result.Should().Be("test");
    }

    [TestMethod]
    public async Task RespondHeadersMediaTypeStringContentStream()
    {
        var ms = new MemoryStream(Encoding.UTF8.GetBytes("test"));

        var response = await _mockHandler.When("/path")
            .Respond(new Dictionary<string, string>
            {
                { "connection", "keep-alive" },
                { "x-hello", "mockhttp" }
            }, "text/plain", ms)
            .SendAsync(_request, CancellationToken.None);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType!.MediaType.Should().Be("text/plain");
        response.Content.ReadAsStringAsync().Result.Should().Be("test");
        response.Headers.Connection.First().Should().Be("keep-alive");
        response.Headers.GetValues("x-hello").First().Should().Be("mockhttp");
    }

    [TestMethod]
    public async Task RespondMediaTypeStringContentString()
    {
        var response = await _mockHandler.When("/path")
            .Respond("text/plain", "test")
            .SendAsync(_request, CancellationToken.None);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType!.MediaType.Should().Be("text/plain");
        response.Content.ReadAsStringAsync().Result.Should().Be("test");
    }

    [TestMethod]
    public async Task RespondHeadersMediaTypeStringContentString()
    {
        var response = await _mockHandler.When("/path")
            .Respond(new Dictionary<string, string>
            {
                { "connection", "keep-alive" },
                { "x-hello", "mockhttp" }
            }, "text/plain", "test")
            .SendAsync(_request, CancellationToken.None);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType!.MediaType.Should().Be("text/plain");
        response.Content.ReadAsStringAsync().Result.Should().Be("test");
        response.Headers.Connection.First().Should().Be("keep-alive");
        response.Headers.GetValues("x-hello").First().Should().Be("mockhttp");
    }

    [TestMethod]
    public async Task RespondHttpStatusCodeMediaTypeStringContentStream()
    {
        var ms = new MemoryStream(Encoding.UTF8.GetBytes("test"));

        var response = await _mockHandler.When("/path")
            .Respond(HttpStatusCode.PartialContent, "text/plain", ms)
            .SendAsync(_request, CancellationToken.None);

        response.StatusCode.Should().Be(HttpStatusCode.PartialContent);
        response.Content.Headers.ContentType!.MediaType.Should().Be("text/plain");
        response.Content.ReadAsStringAsync().Result.Should().Be("test");
    }

    [TestMethod]
    public async Task RespondHttpStatusCodeHeadersMediaTypeStringContentStream()
    {
        var ms = new MemoryStream(Encoding.UTF8.GetBytes("test"));

        var response = await _mockHandler.When("/path")
            .Respond(HttpStatusCode.PartialContent, new Dictionary<string, string>
            {
                { "connection", "keep-alive" },
                { "x-hello", "mockhttp" }
            }, "text/plain", ms)
            .SendAsync(_request, CancellationToken.None);

        response.StatusCode.Should().Be(HttpStatusCode.PartialContent);
        response.Content.Headers.ContentType!.MediaType.Should().Be("text/plain");
        response.Content.ReadAsStringAsync().Result.Should().Be("test");
        response.Headers.Connection.First().Should().Be("keep-alive");
        response.Headers.GetValues("x-hello").First().Should().Be("mockhttp");
    }

    [TestMethod]
    public async Task RespondHttpStatusCodeMediaTypeStringContentString()
    {
        var response = await _mockHandler.When("/path")
            .Respond(HttpStatusCode.PartialContent, "text/plain", "test")
            .SendAsync(_request, CancellationToken.None);

        response.StatusCode.Should().Be(HttpStatusCode.PartialContent);
        response.Content.Headers.ContentType!.MediaType.Should().Be("text/plain");
        response.Content.ReadAsStringAsync().Result.Should().Be("test");
    }

    [TestMethod]
    public async Task RespondHttpStatusCodeHeadersMediaTypeStringContentString()
    {
        var response = await _mockHandler.When("/path")
            .Respond(HttpStatusCode.PartialContent, new Dictionary<string, string>
            {
                { "connection", "keep-alive" },
                { "x-hello", "mockhttp" }
            }, "text/plain", "test")
            .SendAsync(_request, CancellationToken.None);

        response.StatusCode.Should().Be(HttpStatusCode.PartialContent);
        response.Content.Headers.ContentType!.MediaType.Should().Be("text/plain");
        response.Content.ReadAsStringAsync().Result.Should().Be("test");
        response.Headers.Connection.First().Should().Be("keep-alive");
        response.Headers.GetValues("x-hello").First().Should().Be("mockhttp");
    }

    [TestMethod]
    public async Task RespondHttpMessageHandler()
    {
        var passthroughHandler = new MockHttpMessageHandler();
        passthroughHandler.Fallback.Respond(new StringContent("test", Encoding.UTF8, "text/plain"));

        var response = await _mockHandler.When("/path")
            .Respond(passthroughHandler)
            .SendAsync(_request, CancellationToken.None);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType!.MediaType.Should().Be("text/plain");
        response.Content.ReadAsStringAsync().Result.Should().Be("test");
    }

    [TestMethod]
    public async Task RespondHttpMessageHandlerHeaders()
    {
        var passthroughHandler = new MockHttpMessageHandler();
        passthroughHandler.Fallback.Respond(new Dictionary<string, string>
        {
            { "connection", "keep-alive" },
            { "x-hello", "mockhttp" }
        }, new StringContent("test", Encoding.UTF8, "text/plain"));

        var response = await _mockHandler.When("/path")
            .Respond(passthroughHandler)
            .SendAsync(_request, CancellationToken.None);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType!.MediaType.Should().Be("text/plain");
        response.Content.ReadAsStringAsync().Result.Should().Be("test");
        response.Headers.Connection.First().Should().Be("keep-alive");
        response.Headers.GetValues("x-hello").First().Should().Be("mockhttp");
    }

    [TestMethod]
    public async Task RespondHttpClient()
    {
        var passthroughHandler = new MockHttpMessageHandler();
        passthroughHandler.Fallback.Respond(new StringContent("test", Encoding.UTF8, "text/plain"));

        var passthroughClient = new HttpClient(passthroughHandler);

        var response = await _mockHandler.When("/path")
            .Respond(passthroughClient)
            .SendAsync(_request, CancellationToken.None);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType!.MediaType.Should().Be("text/plain");
        response.Content.ReadAsStringAsync().Result.Should().Be("test");
    }

    [TestMethod]
    public void RespondThrowingException()
    {
        var exceptionToThrow = new HttpRequestException("Mocking an HTTP Request Exception.");

        Func<Task> testFunction = async () =>
        {
            await _mockHandler.When("/path")
                .Throw(exceptionToThrow)
                .SendAsync(_request, CancellationToken.None);
        };

        testFunction.Should().ThrowAsync<HttpRequestException>().WithMessage("Mocking an HTTP Request Exception.");
    }
}