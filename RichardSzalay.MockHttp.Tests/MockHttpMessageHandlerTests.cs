using System.Net;
using FluentAssertions;
using RichardSzalay.MockHttp.Enums;
using RichardSzalay.MockHttp.Extensions;

namespace RichardSzalay.MockHttp.Tests;

[TestClass]
public class MockHttpMessageHandlerTests
{
    private MockHttpMessageHandler _mockHandler = null!;
    private HttpClient _instance = null!;

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

        _mockHandler.Fallback.Respond(req => new HttpResponseMessage(HttpStatusCode.OK)
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
        (await result2.Content.ReadAsStringAsync()).Should().Be("{'status' : 'OK'}");
    }

    [TestMethod]
    public async Task ShouldMatchExpectInOrder()
    {
        _mockHandler.Expect("/test")
            .Respond("application/json", "{'status' : 'First'}");

        _mockHandler.Expect("/test")
            .Respond("application/json", "{'status' : 'Second'}");

        var result1 = await _instance.GetAsync("http://invalid/test");
        var result2 = await _instance.GetAsync("http://invalid/test");

        (await result1.Content.ReadAsStringAsync()).Should().Be("{'status' : 'First'}");
        (await result2.Content.ReadAsStringAsync()).Should().Be("{'status' : 'Second'}");
    }

    [TestMethod]
    public async Task ShouldNotMatchExpectOutOfOrder()
    {
        _mockHandler.Expect("/test1")
            .Respond("application/json", "{'status' : 'First'}");

        _mockHandler.Expect("/test2")
            .Respond("application/json", "{'status' : 'Second'}");

        _mockHandler.Fallback.Respond(HttpStatusCode.NotFound);

        var result = await _instance.GetAsync("http://invalid/test2");

        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [TestMethod]
    public void VerifyNoOutstandingExpectationShouldFailIfOutstandingExpectation()
    {
        _mockHandler.Expect("/test")
            .Respond("application/json", "{'status' : 'First'}");

        Action action = () => _mockHandler.VerifyNoOutstandingExpectation();
        action.Should().Throw<InvalidOperationException>();
    }

    [TestMethod]
    public async Task VerifyNoOutstandingExpectationShouldSucceedIfNoOutstandingExpectation()
    {
        _mockHandler.Expect("/test")
            .Respond("application/json", "{'status' : 'First'}");

        var result = await _instance.GetAsync("http://invalid/test");

        Action action = () => _mockHandler.VerifyNoOutstandingExpectation();
        action.Should().NotThrow();
    }

    [TestMethod]
    public async Task ResetExpectationsShouldClearExpectations()
    {
        _mockHandler.When("/test")
            .Respond("application/json", "{'status' : 'OK'}");

        _mockHandler.Expect("/test")
            .Respond("application/json", "{'status' : 'Test'}");

        _mockHandler.ResetExpectations();

        var result = await _instance.GetAsync("http://invalid/test");

        (await result.Content.ReadAsStringAsync()).Should().Be("{'status' : 'OK'}");
    }

    [TestMethod]
    public async Task ShouldNotMatchWhenIfExpectationsExistAndBehavhiorIsNoExpectations()
    {
        _mockHandler.When("/test")
            .Respond(System.Net.HttpStatusCode.OK, "application/json", "{'status' : 'OK'}");

        _mockHandler.Expect("/testA")
            .Respond(System.Net.HttpStatusCode.OK, "application/json", "{'status' : 'Test'}");

        var result = await _instance.GetAsync("http://invalid/test");

        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [TestMethod]
    public void GetMatchCountReturnsZeroWhenNeverCalled()
    {
        var testRequest = _mockHandler.When("/test")
            .Respond("application/json", "{'status' : 'OK'}");

        _mockHandler.GetMatchCount(testRequest).Should().Be(0);
    }

    [TestMethod]
    public async Task GetMatchCountReturnsCallCount()
    {
        var testARequest = _mockHandler.When("/testA")
            .Respond("application/json", "{'status' : 'OK'}");

        var testBRequest = _mockHandler.When("/testB")
            .Respond("application/json", "{'status' : 'OK'}");

        await _instance.GetAsync("http://invalid/testA");
        await _instance.GetAsync("http://invalid/testA");
        await _instance.GetAsync("http://invalid/testB");

        _mockHandler.GetMatchCount(testARequest).Should().Be(2);
        _mockHandler.GetMatchCount(testBRequest).Should().Be(1);
    }

    [TestMethod]
    public async Task ShouldMatchWhenIfExpectationsExistAndBehavhiorIsAlways()
    {
        var mockHandler = new MockHttpMessageHandler(BackendDefinitionBehavior.Always);
        var instance = new HttpClient(mockHandler);

        mockHandler.When("/test")
            .Respond(System.Net.HttpStatusCode.OK, "application/json", "{'status' : 'OK'}");

        mockHandler.Expect("/testA")
            .Respond(System.Net.HttpStatusCode.OK, "application/json", "{'status' : 'Test'}");

        var result = await instance.GetAsync("http://invalid/test");

        var content = await result.Content.ReadAsStringAsync();
        content.Should().Be("{'status' : 'OK'}");
    }

    /// <summary>
    /// ISSUE #29
    /// https://github.com/richardszalay/mockhttp/issues/29
    /// </summary>
    [TestMethod]
    public async Task CanSimulateTimeout()
    {
        var handler = new MockHttpMessageHandler();

        handler.Fallback.Respond(async () =>
        {
            await Task.Delay(10000);

            return new HttpResponseMessage(HttpStatusCode.OK);
        });

        var client = new HttpClient(handler);
        client.Timeout = TimeSpan.FromMilliseconds(1000);
        
        Func<Task> asyncFunc = async () => await client.GetAsync("http://localhost");

        await asyncFunc.Should().ThrowAsync<Exception>();
    }

    /// <summary>
    /// ISSUE #33
    /// https://github.com/richardszalay/mockhttp/issues/33
    /// </summary>
    [TestMethod]
    public async Task DisposingResponseDoesNotFailFutureRequests()
    {
        var handler = new MockHttpMessageHandler();

        handler.When("*").Respond(HttpStatusCode.OK);

        var client = new HttpClient(handler);

        var firstResponse = await client.GetAsync("http://localhost");
        firstResponse.Dispose();
        firstResponse.Should().NotBeNull();
        firstResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var secondResponse = await client.GetAsync("http://localhost");
        secondResponse.Should().NotBeNull();
        secondResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    /// <summary>
    /// ISSUE #39
    /// https://github.com/richardszalay/mockhttp/issues/39
    /// </summary>
    [TestMethod]
    public async Task RespondingWithHttpClientWorks()
    {
        var mockSecondHandler = new MockHttpMessageHandler();
        mockSecondHandler.When("*").Respond(HttpStatusCode.OK);

        var mockFirstHandler = new MockHttpMessageHandler();
        mockFirstHandler.When("*").Respond(mockSecondHandler.ToHttpClient());

        var response = await mockFirstHandler.ToHttpClient().GetAsync("http://localhost/");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}