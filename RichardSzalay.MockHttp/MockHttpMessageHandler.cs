using RichardSzalay.MockHttp.Contracts;
using RichardSzalay.MockHttp.Enums;

namespace RichardSzalay.MockHttp;

public class MockHttpMessageHandler : HttpMessageHandler
{
    private bool autoFlush;
    private int outstandingRequests;
    private TaskCompletionSource<object> flusher = new();

    private readonly BackendDefinitionBehavior backendDefinitionBehavior;
    private readonly Queue<TaskCompletionSource<object>> pendingFlushers = new();
    private readonly Queue<IMockedRequest> requestExpectations = new();
    private readonly List<IMockedRequest> backendDefinitions = new();
    private readonly Dictionary<IMockedRequest, int> _matchCounts = new();
    private readonly object _lockObject = new();
    private readonly MockedRequest fallback;

    /// <summary>
    /// Gets the <see cref="T:MockedRequest"/> that will handle requests that were otherwise unmatched
    /// </summary>
    public MockedRequest Fallback
    {
        get { return fallback; }
    }

    /// <summary>
    /// Creates a new instance of MockHttpMessageHandler
    /// </summary>
    public MockHttpMessageHandler(
        BackendDefinitionBehavior backendDefinitionBehavior = BackendDefinitionBehavior.NoExpectations)
    {
        this.backendDefinitionBehavior = backendDefinitionBehavior;

        AutoFlush = true;
        fallback = new MockedRequest();
        fallback.Respond(CreateDefaultFallbackMessage);
    }


    /// <summary>
    /// Requests received while AutoFlush is true will complete instantly. 
    /// Requests received while AutoFlush is false will not complete until <see cref="M:Flush"/> is called
    /// </summary>
    public bool AutoFlush
    {
        get { return autoFlush; }
        set
        {
            autoFlush = value;

            if (autoFlush)
            {
                flusher = new TaskCompletionSource<object>();
                flusher.SetResult(null!);
            }
            else
            {
                flusher = new TaskCompletionSource<object>();
                pendingFlushers.Enqueue(flusher);
            }
        }
    }

    /// <summary>
    /// Completes all pendings requests that were received while <see cref="M:AutoFlush"/> was false
    /// </summary>
    public void Flush()
    {
        while (pendingFlushers.Count > 0)
            pendingFlushers.Dequeue().SetResult(null!);
    }

    /// <summary>
    /// Completes <param name="count" /> pendings requests that were received while <see cref="M:AutoFlush"/> was false
    /// </summary>
    public void Flush(int count)
    {
        while (pendingFlushers.Count > 0 && count-- > 0)
            pendingFlushers.Dequeue().SetResult(null!);
    }

    /// <summary>
    /// Creates an HttpClient instance using this MockHttpMessageHandler
    /// </summary>
    /// <returns>An instance of HttpClient that can be used to send HTTP request against the configuration of this mock handler</returns>
    public HttpClient ToHttpClient()
    {
        return new HttpClient(this);
    }

    /// <summary>
    /// Maps the request to the most appropriate configured response
    /// </summary>
    /// <param name="request">The request being sent</param>
    /// <param name="cancellationToken">The token used to cancel the request</param>
    /// <returns>A Task containing the future response message</returns>
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
        CancellationToken cancellationToken)
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

        if (backendDefinitionBehavior == BackendDefinitionBehavior.Always
            || requestExpectations.Count == 0)
        {
            foreach (IMockedRequest handler in backendDefinitions.Where(handler => handler.Matches(request)))
            {
                return SendAsync(handler, request, cancellationToken);
            }
        }

        return SendAsync(Fallback, request, cancellationToken);
    }

    private Task<HttpResponseMessage> SendAsync(IMockedRequest handler,
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        Interlocked.Increment(ref outstandingRequests);

        IncrementMatchCount(handler);

        if (!AutoFlush)
        {
            flusher = new TaskCompletionSource<object>();
            pendingFlushers.Enqueue(flusher);
        }

        return flusher.Task.ContinueWith(_ =>
        {
            Interlocked.Decrement(ref outstandingRequests);

            cancellationToken.ThrowIfCancellationRequested();

            var completionSource = new TaskCompletionSource<HttpResponseMessage>();

            cancellationToken.Register(() => completionSource.TrySetCanceled());

            handler.SendAsync(request, cancellationToken)
                .ContinueWith(resp =>
                {
                    resp.Result.RequestMessage = request;

                    if (resp.IsFaulted)
                    {
                        completionSource.TrySetException(resp.Exception);
                    }
                    else if (resp.IsCanceled)
                    {
                        completionSource.TrySetCanceled();
                    }
                    else
                    {
                        completionSource.TrySetResult(resp.Result);
                    }
                });

            return completionSource.Task;
        }).Unwrap();
    }

    private void IncrementMatchCount(IMockedRequest handler)
    {
        lock (_lockObject)
        {
            _matchCounts.TryGetValue(handler, out int count);
            _matchCounts[handler] = count + 1;
        }
    }

    async Task<HttpResponseMessage> CreateDefaultFallbackMessage(HttpRequestMessage req)
    {
        var message = new HttpResponseMessage(System.Net.HttpStatusCode.NotFound)
        {
            ReasonPhrase =
                $"No matching mock handler for \"{req.Method.ToString().ToUpperInvariant()} {req.RequestUri.AbsoluteUri}\""
        };
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
    /// Returns the number of times the specified request specification has been met
    /// </summary>
    /// <param name="request">The mocked request</param>
    /// <returns>The number of times the request has matched</returns>
    public int GetMatchCount(IMockedRequest request)
    {
        lock (_lockObject)
        {
            _matchCounts.TryGetValue(request, out int count);
            return count;
        }
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
        var requests = Interlocked.CompareExchange(ref outstandingRequests, 0, 0);
        if (requests > 0)
            throw new InvalidOperationException("There are " + requests +
                                                " outstanding requests. Call Flush() to complete them");
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