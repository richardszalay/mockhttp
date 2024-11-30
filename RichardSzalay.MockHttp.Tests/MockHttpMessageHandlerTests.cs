using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace RichardSzalay.MockHttp.Tests;

public class MockHttpMessageHandlerTests
{
    [Fact]
    public void Should_respond_with_basic_requests()
    {
        var mockHandler = new MockHttpMessageHandler();

        mockHandler
            .When("/test")
            .Respond("application/json", "{'status' : 'OK'}");

        var result = new HttpClient(mockHandler).GetAsync("http://invalid/test").Result;

        Assert.Equal(System.Net.HttpStatusCode.OK, result.StatusCode);
        Assert.Equal("application/json", result.Content?.Headers.ContentType?.MediaType);
        Assert.Equal("{'status' : 'OK'}", result.Content?.ReadAsStringAsync().Result);
    }

    [Fact]
    public void Should_fall_through_to_next_handler()
    {
        var mockHandler = new MockHttpMessageHandler();
        var client = new HttpClient(mockHandler);

        mockHandler
            .When(HttpMethod.Get, "/test")
            .Respond(System.Net.HttpStatusCode.OK, "application/json", "{'status' : 'OK'}");

        mockHandler
            .When(HttpMethod.Post, "/test")
            .Respond(System.Net.HttpStatusCode.OK, "application/json", "{'status' : 'Boo'}");

        var result = client.PostAsync("http://invalid/test", null).Result;

        Assert.Equal("{'status' : 'Boo'}", result.Content.ReadAsStringAsync().Result);
    }

    [Fact]
    public void Should_assign_request_to_response_object()
    {
        var mockHandler = new MockHttpMessageHandler();
        var client = new HttpClient(mockHandler);

        mockHandler
            .When(HttpMethod.Get, "/test")
            .Respond(System.Net.HttpStatusCode.OK, "application/json", "{'status' : 'OK'}");

        mockHandler
            .When(HttpMethod.Post, "/test")
            .Respond(System.Net.HttpStatusCode.OK, "application/json", "{'status' : 'Boo'}");

        var request = new HttpRequestMessage(HttpMethod.Post, "http://invalid/test");

        var result = client.SendAsync(request).Result;

        Assert.Same(request, result.RequestMessage);
    }

    [Fact]
    public void Manual_flush_should_wait_for_flush()
    {
        var mockHandler = new MockHttpMessageHandler();
        var client = new HttpClient(mockHandler);

        mockHandler
            .When("/test")
            .Respond(System.Net.HttpStatusCode.OK, "application/json", "{'status' : 'OK'}");

        mockHandler.AutoFlush = false;

        bool completed = false;

        var task = client.GetAsync("http://invalid/test")
            .ContinueWith(_ => completed = true);

        Assert.False(completed);

        mockHandler.Flush();

        task.Wait();

        Assert.True(completed);
    }

    [Fact]
    public void VerifyNoOutstandingRequest_should_throw_for_outstanding_requests()
    {
        var mockHandler = new MockHttpMessageHandler();
        var client = new HttpClient(mockHandler);

        mockHandler
            .When("/test")
            .Respond(System.Net.HttpStatusCode.OK, "application/json", "{'status' : 'OK'}");

        mockHandler.AutoFlush = false;

        var task = client.GetAsync("http://invalid/test");

        Assert.Throws<InvalidOperationException>(() => mockHandler.VerifyNoOutstandingRequest());
    }

    [Fact]
    public void VerifyNoOutstandingRequest_should_not_throw_when_outstanding_requests()
    {
        var mockHandler = new MockHttpMessageHandler();
        var client = new HttpClient(mockHandler);

        mockHandler
            .When("/test")
            .Respond(System.Net.HttpStatusCode.OK, "application/json", "{'status' : 'OK'}");

        mockHandler.AutoFlush = false;

        bool completed = false;

        var task = client.GetAsync("http://invalid/test")
            .ContinueWith(_ => completed = true);

        Assert.False(completed);

        mockHandler.Flush();

        task.Wait();

        mockHandler.VerifyNoOutstandingRequest();
    }

    [Fact]
    [Obsolete]
    public void Should_return_fallback_for_unmatched_requests()
    {
        var mockHandler = new MockHttpMessageHandler();
        var client = new HttpClient(mockHandler);

        mockHandler
            .When("/test")
            .Respond(System.Net.HttpStatusCode.OK, "application/json", "{'status' : 'OK'}");

        mockHandler.Fallback.Respond(new HttpResponseMessage(System.Net.HttpStatusCode.OK)
        {
            ReasonPhrase = "Awesome"
        });

        var result = client.GetAsync("http://invalid/test2").Result;

        Assert.Equal("Awesome", result.ReasonPhrase);
    }

    [Fact]
    public async Task Default_fallback_throws_MockHttpMatchException()
    {
        var mockHandler = new MockHttpMessageHandler();
        var client = new HttpClient(mockHandler);

        mockHandler
            .When("/test")
            .Respond(System.Net.HttpStatusCode.OK, "application/json", "{'status' : 'OK'}");

        var result = await Assert.ThrowsAsync<MockHttpMatchException>(() => client.GetAsync("http://invalid/test2"));

        Assert.StartsWith("Failed to match a mocked request for GET http://invalid/test2", result.Message);
    }

    [Fact]
    public void Should_match_expect_before_when()
    {
        var mockHandler = new MockHttpMessageHandler();
        var client = new HttpClient(mockHandler);

        mockHandler
            .When("/test")
            .Respond(System.Net.HttpStatusCode.OK, "application/json", "{'status' : 'OK'}");

        mockHandler
            .Expect("/test")
            .Respond(System.Net.HttpStatusCode.OK, "application/json", "{'status' : 'Test'}");

        var result1 = client.GetAsync("http://invalid/test").Result;
        var result2 = client.GetAsync("http://invalid/test").Result;

        Assert.Equal("{'status' : 'Test'}", result1.Content.ReadAsStringAsync().Result);
        Assert.Equal("{'status' : 'OK'}", result2.Content.ReadAsStringAsync().Result);
    }

    [Fact]
    public void Should_match_expect_in_order()
    {
        var mockHandler = new MockHttpMessageHandler();
        var client = new HttpClient(mockHandler);

        mockHandler
            .Expect("/test")
            .Respond("application/json", "{'status' : 'First'}");

        mockHandler
            .Expect("/test")
            .Respond("application/json", "{'status' : 'Second'}");

        var result1 = client.GetAsync("http://invalid/test").Result;
        var result2 = client.GetAsync("http://invalid/test").Result;

        Assert.Equal("{'status' : 'First'}", result1.Content.ReadAsStringAsync().Result);
        Assert.Equal("{'status' : 'Second'}", result2.Content.ReadAsStringAsync().Result);
    }

    [Fact]
    public void Should_not_match_expect_out_of_order()
    {
        var mockHandler = new MockHttpMessageHandler();
        var client = new HttpClient(mockHandler);

        mockHandler
            .Expect("/test1")
            .Respond("application/json", "{'status' : 'First'}");

        mockHandler
            .Expect("/test2")
            .Respond("application/json", "{'status' : 'Second'}");

        mockHandler.Fallback.Respond(HttpStatusCode.NotFound);

        var result = client.GetAsync("http://invalid/test2").Result;

        Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
    }

    [Fact]
    public async Task Should_work_with_multiple_concurrent_writes_to_queue()
    {
        var mockHandler = new MockHttpMessageHandler();
        var tasks = new ConcurrentQueue<Task>();
        var readyEvent = new ManualResetEvent(false);

        for (int i = 0; i < 100; i++)
        {
            tasks.Enqueue(Task.Run(async () =>
            {
                await Task.Yield();
                readyEvent.WaitOne();
                mockHandler
                    .Expect("/test")
                    .Respond("application/json", "{'status' : 'First'}");
            }));
        }
        readyEvent.Set();

        await Task.WhenAll(tasks).ConfigureAwait(false);
    }

    [Fact]
    public void VerifyNoOutstandingExpectation_should_fail_if_outstanding_expectation()
    {
        var mockHandler = new MockHttpMessageHandler();
        var client = new HttpClient(mockHandler);

        mockHandler
            .Expect("/test")
            .Respond("application/json", "{'status' : 'First'}");

        Assert.Throws<InvalidOperationException>(() => mockHandler.VerifyNoOutstandingExpectation());
    }

    [Fact]
    public void VerifyNoOutstandingExpectation_should_succeed_if_no_outstanding_expectation()
    {
        var mockHandler = new MockHttpMessageHandler();
        var client = new HttpClient(mockHandler);

        mockHandler
            .Expect("/test")
            .Respond("application/json", "{'status' : 'First'}");

        var result = client.GetAsync("http://invalid/test").Result;

        mockHandler.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public void ResetExpectations_should_clear_expectations()
    {
        var mockHandler = new MockHttpMessageHandler();
        var client = new HttpClient(mockHandler);

        mockHandler
            .When("/test")
            .Respond("application/json", "{'status' : 'OK'}");

        mockHandler
            .Expect("/test")
            .Respond("application/json", "{'status' : 'Test'}");

        mockHandler.ResetExpectations();

        var result = client.GetAsync("http://invalid/test").Result;

        Assert.Equal("{'status' : 'OK'}", result.Content.ReadAsStringAsync().Result);
    }

    [Fact]
    public void Should_not_match_when_if_expectations_exist_and_behavhior_is_NoExpectations()
    {
        var mockHandler = new MockHttpMessageHandler();
        var client = new HttpClient(mockHandler);

        mockHandler
            .When("/test")
            .Respond(System.Net.HttpStatusCode.OK, "application/json", "{'status' : 'OK'}");

        mockHandler
            .Expect("/testA")
            .Respond(System.Net.HttpStatusCode.OK, "application/json", "{'status' : 'Test'}");

        mockHandler.Fallback.RespondMatchSummary();

        var result = client.GetAsync("http://invalid/test").Result;

        Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
    }

    [Fact]
    public void Should_match_when_if_expectations_exist_and_behavhior_is_Always()
    {
        var mockHandler = new MockHttpMessageHandler(BackendDefinitionBehavior.Always);
        var client = new HttpClient(mockHandler);

        mockHandler
            .When("/test")
            .Respond(System.Net.HttpStatusCode.OK, "application/json", "{'status' : 'OK'}");

        mockHandler
            .Expect("/testA")
            .Respond(System.Net.HttpStatusCode.OK, "application/json", "{'status' : 'Test'}");

        var result = client.GetAsync("http://invalid/test").Result;

        Assert.Equal("{'status' : 'OK'}", result.Content.ReadAsStringAsync().Result);
    }

    [Fact]
    public void GetMatchCount_returns_zero_when_never_called()
    {
        var mockHandler = new MockHttpMessageHandler();
        var client = new HttpClient(mockHandler);

        var testRequest = mockHandler
            .When("/test")
            .Respond("application/json", "{'status' : 'OK'}");

        Assert.Equal(0, mockHandler.GetMatchCount(testRequest));
    }

    [Fact]
    public async Task GetMatchCount_returns_call_count()
    {
        var mockHandler = new MockHttpMessageHandler();
        var client = new HttpClient(mockHandler);

        var testARequest = mockHandler
            .When("/testA")
            .Respond("application/json", "{'status' : 'OK'}");

        var testBRequest = mockHandler
            .When("/testB")
            .Respond("application/json", "{'status' : 'OK'}");

        await client.GetAsync("http://invalid/testA");
        await client.GetAsync("http://invalid/testA");
        await client.GetAsync("http://invalid/testB");

        Assert.Equal(2, mockHandler.GetMatchCount(testARequest));
        Assert.Equal(1, mockHandler.GetMatchCount(testBRequest));
    }
}
