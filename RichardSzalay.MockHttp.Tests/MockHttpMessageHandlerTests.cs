using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using RichardSzalay.MockHttp;
using Xunit;

namespace RichardSzalay.MockHttp.Tests
{
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
            Assert.Equal("application/json", result.Content.Headers.ContentType.MediaType);
            Assert.Equal("{'status' : 'OK'}", result.Content.ReadAsStringAsync().Result);
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

            Assert.DoesNotThrow(() => mockHandler.VerifyNoOutstandingRequest());
        }

        [Fact]
        public void Should_return_fallbackresponse_for_unmatched_requests()
        {
            var mockHandler = new MockHttpMessageHandler();
            var client = new HttpClient(mockHandler);

            mockHandler
                .When("/test")
                .Respond(System.Net.HttpStatusCode.OK, "application/json", "{'status' : 'OK'}");

            mockHandler.FallbackResponse = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                ReasonPhrase = "Awesome"
            };

            var result = client.GetAsync("http://invalid/test2").Result;

            Assert.Equal("Awesome", result.ReasonPhrase);
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

            var result = client.GetAsync("http://invalid/test2").Result;

            Assert.Equal(mockHandler.FallbackResponse.StatusCode, result.StatusCode);
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

            Assert.DoesNotThrow(mockHandler.VerifyNoOutstandingExpectation);
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
        public void FallbackResponseHandler_should_be_null_by_default()
        {
            var mockHandler = new MockHttpMessageHandler();

            var actualFallbackResponseHandler = mockHandler.FallbackResponseHandler;

            Assert.Null(actualFallbackResponseHandler);
        }

        [Fact]
        public void Should_return_FallbackResponseHandler_for_unmatched_requests()
        {
            var mockHandler = new MockHttpMessageHandler();
            var client = new HttpClient(mockHandler);

            mockHandler
                .When("/test")
                .Respond(HttpStatusCode.OK, "application/json", "{'status' : 'OK'}");

            mockHandler.FallbackResponseHandler = (HttpRequestMessage request) =>
                {
                    string message = String.Format(
                        "Handler for request {0} {1} was not matched by any mock handler.",
                        request.Method,
                        request.RequestUri);

                    var response = new HttpResponseMessage(HttpStatusCode.NotFound);
                    response.Content = new StringContent(message, Encoding.UTF8, "text/plain");
                    return response;
                };

            var result = client.GetAsync("http://invalid/test2").Result;
            var actualResponseContent = result.Content.ReadAsStringAsync().Result;

            Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
            Assert.Equal("Handler for request GET http://invalid/test2 was not matched by any mock handler.", actualResponseContent);
        }
    }
}
