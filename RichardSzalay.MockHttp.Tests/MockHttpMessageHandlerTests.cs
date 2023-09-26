using FluentAssertions;
using RichardSzalay.MockHttp.Extensions;

namespace RichardSzalay.MockHttp.Tests;

[TestClass]
public class MockHttpMessageHandlerTests
{
    private MockHttpMessageHandler _mockHandler;
    private HttpClient _instance;

    [TestInitialize]
    public void SetUp()
    {
        _mockHandler = new MockHttpMessageHandler();

        _instance = new HttpClient(_mockHandler);
    }

    [TestMethod]
    public async Task ShouldRespondWithBasicRequests()
    {
        _mockHandler.When("/test")
            .Respond("application/json", "{'status' : 'OK'}");

        var result = await _instance.GetAsync("http://invalid/test");

        result.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        result.Content.Headers.ContentType.Should().NotBeNull();
        result.Content.Headers.ContentType!.MediaType.Should().Be("application/json");
        (await result.Content.ReadAsStringAsync()).Should().Be("{'status' : 'OK'}");
    }

    [TestMethod]
    public async Task ShouldFallThroughToNextHandler()
    {
        _mockHandler.When(HttpMethod.Get, "/test")
            .Respond(System.Net.HttpStatusCode.OK, "application/json", "{'status' : 'OK'}");

        _mockHandler.When(HttpMethod.Post, "/test")
            .Respond(System.Net.HttpStatusCode.OK, "application/json", "{'status' : 'Boo'}");

        var result = await _instance.PostAsync("http://invalid/test", null);

        var content = await result.Content.ReadAsStringAsync();
        content.Should().Be("{'status' : 'Boo'}");
    }

    [TestMethod]
    public async Task ShouldAssignRequestToResponseObject()
    {
        _mockHandler.When(HttpMethod.Get, "/test")
            .Respond(System.Net.HttpStatusCode.OK, "application/json", "{'status' : 'OK'}");

        _mockHandler.When(HttpMethod.Post, "/test")
            .Respond(System.Net.HttpStatusCode.OK, "application/json", "{'status' : 'Boo'}");

        var request = new HttpRequestMessage(HttpMethod.Post, "http://invalid/test");

        var result = await _instance.SendAsync(request);

        result.RequestMessage.Should().NotBeNull();
        result.RequestMessage.Should().BeSameAs(request);
    }

    [TestMethod]
    public void ManualFlushShouldWaitForFlush()
    {
        _mockHandler.When("/test")
            .Respond(System.Net.HttpStatusCode.OK, "application/json", "{'status' : 'OK'}");
        _mockHandler.AutoFlush = false;

        bool completed = false;

        var task = _instance.GetAsync("http://invalid/test")
            .ContinueWith(_ => completed = true);

        completed.Should().BeFalse();

        _mockHandler.Flush();

        task.Wait();

        completed.Should().BeTrue();
    }

    [TestMethod]
    public void VerifyNoOutstandingRequestShouldThrowForOutstandingRequests()
    {
        _mockHandler.When("/test")
            .Respond(System.Net.HttpStatusCode.OK, "application/json", "{'status' : 'OK'}");

        _mockHandler.AutoFlush = false;

        var task = _instance.GetAsync("http://invalid/test");

        Action action = () => _mockHandler.VerifyNoOutstandingRequest();
        action.Should().Throw<InvalidOperationException>();
    }

    [TestMethod]
    public void VerifyNoOutstandingRequestShouldNotThrowWhenOutstandingRequests()
    {
        _mockHandler.When("/test")
            .Respond(System.Net.HttpStatusCode.OK, "application/json", "{'status' : 'OK'}");

        _mockHandler.AutoFlush = false;

        bool completed = false;

        var task = _instance.GetAsync("http://invalid/test")
            .ContinueWith(_ => completed = true);

        completed.Should().BeFalse();

        _mockHandler.Flush();

        task.Wait();

        Action action = () => _mockHandler.VerifyNoOutstandingRequest();
        action.Should().NotThrow();
    }

    [TestMethod]
    public async Task ShouldReturnFallbackForUnmatchedRequests()
    {
        _mockHandler.When("/test")
            .Respond(System.Net.HttpStatusCode.OK, "application/json", "{'status' : 'OK'}");

        _mockHandler.Fallback.Respond(new HttpResponseMessage(System.Net.HttpStatusCode.OK)
        {
            ReasonPhrase = "Awesome"
        });

        var result = await _instance.GetAsync("http://invalid/test2");

        result.ReasonPhrase.Should().Be("Awesome");
    }

    [TestMethod]
    public async Task DefaultFallbackIncludesMethodAndUrl()
    {
        _mockHandler.When("/test")
            .Respond(System.Net.HttpStatusCode.OK, "application/json", "{'status' : 'OK'}");

        var result = await _instance.GetAsync("http://invalid/test2");

        result.ReasonPhrase.Should().Be("No matching mock handler for \"GET http://invalid/test2\"");
    }

    [TestMethod]
    public async Task ShouldMatchExpectBeforeWhen()
    {
        _mockHandler.When("/test")
            .Respond(System.Net.HttpStatusCode.OK, "application/json", "{'status' : 'OK'}");

        _mockHandler.Expect("/test")
            .Respond(System.Net.HttpStatusCode.OK, "application/json", "{'status' : 'Test'}");

        var result1 = await _instance.GetAsync("http://invalid/test");
        var result2 = await _instance.GetAsync("http://invalid/test");

        (await result1.Content.ReadAsStringAsync()).Should().Be("{'status' : 'Test'}");
        (await result2.Content.ReadAsStringAsync()).Should().Be("{'status' : 'Ok'}");
    }
}