using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RichardSzalay.MockHttp
{
    /// <summary>
    /// Responds to requests using pre-configured responses
    /// </summary>
    public class MockHttpMessageHandler : HttpMessageHandler
    {
        private Queue<IMockedRequest> requestExpectations = new Queue<IMockedRequest>();
        private List<IMockedRequest> backendDefinitions = new List<IMockedRequest>();



        private int outstandingRequests = 0;

        /// <summary>
        /// Creates a new instance of MockHttpMessageHandler
        /// </summary>
        public MockHttpMessageHandler()
        {
            AutoFlush = true;
            fallback = new MockedRequest();
            fallback.Respond(fallbackResponse = CreateDefaultFallbackMessage());
        }

        private bool autoFlush;

        /// <summary>
        /// Requests received while AutoFlush is true will complete instantly. 
        /// Requests received while AutoFlush is false will not complete until <see cref="M:Flush"/> is called
        /// </summary>
        public bool AutoFlush
        {
            get
            {
                return autoFlush;
            }
            set
            {
                autoFlush = value;

                if (autoFlush)
                {
                    flusher = new TaskCompletionSource<object>();
                    flusher.SetResult(null);
                }
                else
                {
                    flusher = new TaskCompletionSource<object>();
                    pendingFlushers.Enqueue(flusher);
                }
            }
        }

        private Queue<TaskCompletionSource<object>> pendingFlushers = new Queue<TaskCompletionSource<object>>();
        private TaskCompletionSource<object> flusher;

        /// <summary>
        /// Completes all pendings requests that were received while <see cref="M:AutoFlush"/> was false
        /// </summary>
        public void Flush()
        {
            while (pendingFlushers.Count > 0)
                pendingFlushers.Dequeue().SetResult(null);
        }

        /// <summary>
        /// Completes <param name="count" /> pendings requests that were received while <see cref="M:AutoFlush"/> was false
        /// </summary>
        public void Flush(int count)
        {
            while (pendingFlushers.Count > 0 && count-- > 0)
                pendingFlushers.Dequeue().SetResult(null);
        }

        /// <summary>
        /// Maps the request to the most appropriate configured response
        /// </summary>
        /// <param name="request">The request being sent</param>
        /// <param name="cancellationToken">The token used to cancel the request</param>
        /// <returns>A Task containing the future response message</returns>
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (requestExpectations.Count > 0)
            {
                var handler = requestExpectations.Peek();

                if (handler.Matches(request))
                {
                    requestExpectations.Dequeue();

                    return SendAsync(handler, request, cancellationToken);
                }
            }
            else
            {
                foreach (var handler in backendDefinitions)
                {
                    if (handler.Matches(request))
                    {
                        return SendAsync(handler, request, cancellationToken);
                    }
                }
            }

            return SendAsync(Fallback, request, cancellationToken);
        }

        private Task<HttpResponseMessage> SendAsync(IMockedRequest handler, HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Interlocked.Increment(ref outstandingRequests);

            if (!AutoFlush)
            {
                flusher = new TaskCompletionSource<object>();
                pendingFlushers.Enqueue(flusher);
            }

            return flusher.Task.ContinueWith(_ =>
                {
                    Interlocked.Decrement(ref outstandingRequests);

                    return handler.SendAsync(request, cancellationToken)
                        .ContinueWith(resp =>
                        {
                            resp.Result.RequestMessage = request;
                            return resp.Result;
                        });
                }).Unwrap();
        }

        private HttpResponseMessage fallbackResponse = null;
        private MockedRequest fallback;

        /// <summary>
        /// Gets the <see cref="T:MockedRequest"/> that will handle requests that were otherwise unmatched
        /// </summary>
        public MockedRequest Fallback
        {
            get
            {
                return fallback;
            }
        }

        /// <summary>
        /// Gets or sets the response that will be returned for requests that were not matched
        /// </summary>
        [Obsolete("Please use Fallback. FallbackResponse will be removed in a future release")]
        public HttpResponseMessage FallbackResponse
        {
            get
            {
                return fallbackResponse;
            }
            set
            {
                if (value == null)
                    throw new ArgumentNullException();

                fallbackResponse = value;
                fallback.Respond(value);
            }
        }

        HttpResponseMessage CreateDefaultFallbackMessage()
        {
            HttpResponseMessage message = new HttpResponseMessage(System.Net.HttpStatusCode.NotFound);
            message.ReasonPhrase = "No matching mock handler";
            return message;
        }

        /// <summary>
        /// Adds a request expectation
        /// </summary>
        /// <remarks>
        /// Request expectations:
        /// 
        /// <list>
        /// <item>Match once</item>
        /// <item>Match in order</item>
        /// <item>Match before any backend definitions</item>
        /// </list>
        /// </remarks>
        /// <param name="handler">The <see cref="T:IMockedRequest"/> that will handle the request</param>
        public void AddRequestExpectation(IMockedRequest handler)
        {
            requestExpectations.Enqueue(handler);
        }

        /// <summary>
        /// Adds a backend definition
        /// </summary>
        /// <remarks>
        /// Backend definitions:
        /// 
        /// <list>
        /// <item>Match multiple times</item>
        /// <item>Match in any order</item>
        /// <item>Match after all request expectations have been met</item>
        /// </list>
        /// </remarks>
        /// <param name="handler">The <see cref="T:IMockedRequest"/> that will handle the request</param>
        public void AddBackendDefinition(IMockedRequest handler)
        {
            backendDefinitions.Add(handler);
        }

        /// <summary>
        /// Disposes the current instance
        /// </summary>
        /// <param name="disposing">true if called from Dispose(); false if called from dtor()</param>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }

        /// <summary>
        /// Throws an <see cref="T:InvalidOperationException"/> if there are requests that were received 
        /// while <see cref="M:AutoFlush"/> was true, but have not been completed using <see cref="M:Flush"/>
        /// </summary>
        public void VerifyNoOutstandingRequest()
        {
            if (outstandingRequests > 0)
                throw new InvalidOperationException("There are " + outstandingRequests + " oustanding requests. Call Flush() to complete them");
        }

        /// <summary>
        /// Throws an <see cref="T:InvalidOperationException"/> if there are any requests configured with Expects 
        /// that have yet to be received
        /// </summary>
        public void VerifyNoOutstandingExpectation()
        {
            if (this.requestExpectations.Count > 0)
                throw new InvalidOperationException("There are " + requestExpectations.Count + " unfulfilled expectations");
        }

        /// <summary>
        /// Clears any pending requests configured with Expect
        /// </summary>
        public void ResetExpectations()
        {
            this.requestExpectations.Clear();
        }

        /// <summary>
        /// Clears any mocked requests configured with When
        /// </summary>
        public void ResetBackendDefinitions()
        {
            this.backendDefinitions.Clear();
        }

        /// <summary>
        /// Clears all mocked requests configured with either Expect or When
        /// </summary>
        public void Clear()
        {
            this.ResetExpectations();
            this.ResetBackendDefinitions();
        }
    }
}
