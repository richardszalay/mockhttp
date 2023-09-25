﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using RichardSzalay.MockHttp;
using System.Net.Http.Headers;
using RichardSzalay.MockHttp.Matchers;
using System.Threading;
using System.Net;
using System.IO;

namespace RichardSzalay.MockHttp.Tests
{
    /// <summary>
    /// Sanity check tests for MockedRequest.Respond() extension methods.
    /// </summary>
    public class MockedRequestExtentionsRespondTests
    {
        [Fact]
        public void Respond_HttpContent()
        {
            var response = Test(r => r.Respond(new StringContent("test", Encoding.UTF8, "text/plain")));

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("text/plain", response.Content.Headers.ContentType.MediaType);
            Assert.Equal("test", response.Content.ReadAsStringAsync().Result);
        }

        [Fact]
        public void Respond_HttpContent_Unique()
        {
            var mockHttp = new MockHttpMessageHandler();

            var mockedRequest = mockHttp.When("/path");
            mockedRequest.Respond(req => new StringContent("test", Encoding.UTF8, "text/plain"));

            var contentA = mockedRequest.SendAsync(request, CancellationToken.None).Result.Content;
            var contentB = mockedRequest.SendAsync(request, CancellationToken.None).Result.Content;

            Assert.NotSame(contentA, contentB);
        }

        [Fact]
        public void Respond_String_Unique()
        {
            var mockHttp = new MockHttpMessageHandler();

            var mockedRequest = mockHttp.When("/path");
            mockedRequest.Respond("application/json", "{\"test\":\"value\"}");

            var contentA = mockedRequest.SendAsync(request, CancellationToken.None).Result.Content;
            var contentB = mockedRequest.SendAsync(request, CancellationToken.None).Result.Content;

            Assert.NotSame(contentA, contentB);
        }

        [Fact]
        public void Respond_Stream_Reread_If_Possible()
        {
            var mockHttp = new MockHttpMessageHandler();

            var mockedRequest = mockHttp.When("/path");
            mockedRequest.Respond("text/plain", new MemoryStream(Encoding.UTF8.GetBytes("test")));

            var contentA = mockedRequest.SendAsync(request, CancellationToken.None).Result.Content.ReadAsStringAsync().Result;
            var contentB = mockedRequest.SendAsync(request, CancellationToken.None).Result.Content.ReadAsStringAsync().Result;

            Assert.NotSame(contentA, contentB);
        }

        [Fact]
        public void Respond_Stream_Handler()
        {
            var mockHttp = new MockHttpMessageHandler();

            var mockedRequest = mockHttp.When("/path");
            mockedRequest.Respond("text/plain", req => new MemoryStream(Encoding.UTF8.GetBytes("test")));

            var contentA = mockedRequest.SendAsync(request, CancellationToken.None).Result.Content.ReadAsStringAsync().Result;
            var contentB = mockedRequest.SendAsync(request, CancellationToken.None).Result.Content.ReadAsStringAsync().Result;

            Assert.NotSame(contentA, contentB);
        }

        [Fact]
        public void Respond_HttpMessage()
        {
            var expected = new HttpResponseMessage();
            var response = Test(r => r.Respond(expected));

            Assert.Same(expected, response);
        }

        [Fact]
        public void Respond_HttpStatusCode()
        {
            var response = Test(r => r.Respond(HttpStatusCode.NoContent));

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public void Respond_FuncRequestResponse()
        {
            var expected = new HttpResponseMessage();
            var response = Test(r => r.Respond(req => expected));

            Assert.Same(expected, response);
        }

        [Fact]
        public void Respond_FuncRequestAsyncResponse()
        {
            var expected = new HttpResponseMessage();
            var response = Test(r => r.Respond(req => Task.FromResult(expected)));

            Assert.Same(expected, response);
        }

        [Fact]
        public void Respond_HttpStatus_HttpContent()
        {
            var response = Test(r => r.Respond(HttpStatusCode.Found, new StringContent("test", Encoding.UTF8, "text/plain")));

            Assert.Equal(HttpStatusCode.Found, response.StatusCode);
            Assert.Equal("text/plain", response.Content.Headers.ContentType.MediaType);
            Assert.Equal("test", response.Content.ReadAsStringAsync().Result);
        }

        [Fact]
        public void Respond_HttpStatus_headers_HttpContent()
        {
            var response = Test(r => r.Respond(HttpStatusCode.Found, new Dictionary<string, string> {
                { "connection", "keep-alive" },
                { "x-hello", "mockhttp" }
            }, new StringContent("test", Encoding.UTF8, "text/plain")));

            Assert.Equal(HttpStatusCode.Found, response.StatusCode);
            Assert.Equal("text/plain", response.Content.Headers.ContentType.MediaType);
            Assert.Equal("test", response.Content.ReadAsStringAsync().Result);
            Assert.Equal("keep-alive", response.Headers.Connection.First());
            Assert.Equal("mockhttp", response.Headers.GetValues("x-hello").First());
        }

        [Fact]
        public void Respond_mediaTypeString_contentStream()
        {
            MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes("test"));

            var response = Test(r => r.Respond("text/plain", ms));

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("text/plain", response.Content.Headers.ContentType.MediaType);
            Assert.Equal("test", response.Content.ReadAsStringAsync().Result);
        }

        [Fact]
        public void Respond_headers_mediaTypeString_contentStream()
        {
            MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes("test"));

            var response = Test(r => r.Respond(new Dictionary<string, string> {
                { "connection", "keep-alive" },
                { "x-hello", "mockhttp" }
            }, "text/plain", ms));

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("text/plain", response.Content.Headers.ContentType.MediaType);
            Assert.Equal("test", response.Content.ReadAsStringAsync().Result);
            Assert.Equal("keep-alive", response.Headers.Connection.First());
            Assert.Equal("mockhttp", response.Headers.GetValues("x-hello").First());
        }

        [Fact]
        public void Respond_mediaTypeString_contentString()
        {
            var response = Test(r => r.Respond("text/plain", "test"));

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("text/plain", response.Content.Headers.ContentType.MediaType);
            Assert.Equal("test", response.Content.ReadAsStringAsync().Result);
        }

        [Fact]
        public void Respond_headers_mediaTypeString_contentString()
        {
            var response = Test(r => r.Respond(new Dictionary<string, string> {
                { "connection", "keep-alive" },
                { "x-hello", "mockhttp" }
            }, "text/plain", "test"));

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("text/plain", response.Content.Headers.ContentType.MediaType);
            Assert.Equal("test", response.Content.ReadAsStringAsync().Result);
            Assert.Equal("keep-alive", response.Headers.Connection.First());
            Assert.Equal("mockhttp", response.Headers.GetValues("x-hello").First());
        }

        [Fact]
        public void Respond_HttpStatusCode_mediaTypeString_contentStream()
        {
            MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes("test"));

            var response = Test(r => r.Respond(HttpStatusCode.PartialContent, "text/plain", ms));

            Assert.Equal(HttpStatusCode.PartialContent, response.StatusCode);
            Assert.Equal("text/plain", response.Content.Headers.ContentType.MediaType);
            Assert.Equal("test", response.Content.ReadAsStringAsync().Result);
        }


        [Fact]
        public void Respond_HttpStatusCode_headers_mediaTypeString_contentStream()
        {
            MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes("test"));

            var response = Test(r => r.Respond(HttpStatusCode.PartialContent, new Dictionary<string, string> {
                { "connection", "keep-alive" },
                { "x-hello", "mockhttp" }
            }, "text/plain", ms));

            Assert.Equal(HttpStatusCode.PartialContent, response.StatusCode);
            Assert.Equal("text/plain", response.Content.Headers.ContentType.MediaType);
            Assert.Equal("test", response.Content.ReadAsStringAsync().Result);
            Assert.Equal("keep-alive", response.Headers.Connection.First());
            Assert.Equal("mockhttp", response.Headers.GetValues("x-hello").First());
        }

        [Fact]
        public void Respond_HttpStatusCode_mediaTypeString_contentString()
        {
            var response = Test(r => r.Respond(HttpStatusCode.PartialContent, "text/plain", "test"));

            Assert.Equal(HttpStatusCode.PartialContent, response.StatusCode);
            Assert.Equal("text/plain", response.Content.Headers.ContentType.MediaType);
            Assert.Equal("test", response.Content.ReadAsStringAsync().Result);
        }

        [Fact]
        public void Respond_HttpStatusCode_headers_mediaTypeString_contentString()
        {
            var response = Test(r => r.Respond(HttpStatusCode.PartialContent, new Dictionary<string, string> {
                { "connection", "keep-alive" },
                { "x-hello", "mockhttp" }
            },  "text/plain", "test"));

            Assert.Equal(HttpStatusCode.PartialContent, response.StatusCode);
            Assert.Equal("text/plain", response.Content.Headers.ContentType.MediaType);
            Assert.Equal("test", response.Content.ReadAsStringAsync().Result);
            Assert.Equal("keep-alive", response.Headers.Connection.First());
            Assert.Equal("mockhttp", response.Headers.GetValues("x-hello").First());
        }

        [Fact]
        public void Respond_HttpMessageHandler()
        {
            var passthroughHandler = new MockHttpMessageHandler();
            passthroughHandler.Fallback.Respond(new StringContent("test", Encoding.UTF8, "text/plain"));

            var response = Test(r => r.Respond(passthroughHandler));

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("text/plain", response.Content.Headers.ContentType.MediaType);
            Assert.Equal("test", response.Content.ReadAsStringAsync().Result);
        }

        [Fact]
        public void Respond_HttpMessageHandler_headers()
        {
            var passthroughHandler = new MockHttpMessageHandler();
            passthroughHandler.Fallback.Respond(new Dictionary<string, string> {
                { "connection", "keep-alive" },
                { "x-hello", "mockhttp" }
            }, new StringContent("test", Encoding.UTF8, "text/plain"));

            var response = Test(r => r.Respond(passthroughHandler));

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("text/plain", response.Content.Headers.ContentType.MediaType);
            Assert.Equal("test", response.Content.ReadAsStringAsync().Result);
            Assert.Equal("keep-alive", response.Headers.Connection.First());
            Assert.Equal("mockhttp", response.Headers.GetValues("x-hello").First());
        }

        [Fact]
        public void Respond_HttpClient()
        {
            var passthroughHandler = new MockHttpMessageHandler();
            passthroughHandler.Fallback.Respond(new StringContent("test", Encoding.UTF8, "text/plain"));

            var passthroughClient = new HttpClient(passthroughHandler);

            var response = Test(r => r.Respond(passthroughClient));

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("text/plain", response.Content.Headers.ContentType.MediaType);
            Assert.Equal("test", response.Content.ReadAsStringAsync().Result);
        }

        [Fact]
        public void Respond_Throwing_Exception()
        {
            var exceptionToThrow = new HttpRequestException("Mocking an HTTP Request Exception.");

            Assert.Throws<HttpRequestException>(() => Test(r => r.Throw(exceptionToThrow)));
        }

        private HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "http://www.tempuri.org/path?apple=red&pear=green")
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "data1", "value1" },
                { "data2", "value2" }
            })
        };

        private HttpResponseMessage Test(Action<MockedRequest> respond)
        {
            var mockHttp = new MockHttpMessageHandler();

            var mockedRequest = mockHttp.When("/path");
            respond(mockedRequest);
            
            return mockedRequest.SendAsync(request, CancellationToken.None).Result;
        }
    }
}
