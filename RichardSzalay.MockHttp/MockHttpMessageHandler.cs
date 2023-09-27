using System.Net;
using RichardSzalay.MockHttp.Contracts;
using RichardSzalay.MockHttp.Enums;

namespace RichardSzalay.MockHttp;

public class MockHttpMessageHandler : HttpMessageHandler
{
    private int _outstandingRequests;
    private TaskCompletionSource<object> _flusher = new();
    private readonly BackendDefinitionBehavior _backendDefinitionBehavior;
    private readonly Queue<TaskCompletionSource<object>> _pendingFlushers = new();
    private readonly Queue<IMockedRequest> _requestExpectations = new();
    private readonly List<IMockedRequest> _backendDefinitions = new();
    private readonly Dictionary<IMockedRequest, int> _matchCounts = new();
    private readonly object _lockObject = new();

    private readonly MockedRequest _fallback;

    /// <summary>
    /// Gets the <see cref="T:MockedRequest"/> that will handle requests that were otherwise unmatched
    /// </summary>
    public MockedRequest Fallback
    {
        get { return this._fallback; }
    }

    private bool _autoFlush;
    /// <summary>
    /// Requests received while AutoFlush is true will complete instantly. 
    /// Requests received while AutoFlush is false will not complete until <see cref="M:Flush"/> is called
    /// </summary>
    public bool AutoFlush
    {
        get => this._autoFlush;
        set
        {
            this._autoFlush = value;

            if (this._autoFlush)
            {
                this._flusher = new TaskCompletionSource<object>();
                this._flusher.SetResult(null!);
            }
            else
            {
                this._flusher = new TaskCompletionSource<object>();
                this._pendingFlushers.Enqueue(this._flusher);
            }
        }
    }

    /// <summary>
    /// Creates a new instance of MockHttpMessageHandler
    /// </summary>
    public MockHttpMessageHandler(BackendDefinitionBehavior backendDefinitionBehavior = BackendDefinitionBehavior.NoExpectations)
    {
        this._backendDefinitionBehavior = backendDefinitionBehavior;

        this.AutoFlush = true;
        this._fallback = new MockedRequest();
        this._fallback.Respond(CreateDefaultFallbackMessage);
    }

    /// <summary>
    /// Completes all pendings requests that were received while <see cref="M:AutoFlush"/> was false
    /// </summary>
    public void Flush()
    {
        while (this._pendingFlushers.Count > 0)
        {
            this._pendingFlushers.Dequeue().SetResult(null!);
        }
    }

    /// <summary>
    /// Completes <param name="count" /> pendings requests that were received while <see cref="M:AutoFlush"/> was false
    /// </summary>
    public void Flush(int count)
    {
        while (this._pendingFlushers.Count > 0
               && count-- > 0)
        {
            this._pendingFlushers.Dequeue().SetResult(null!);
        }
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
        if (this._requestExpectations.Count > 0)
        {
            IMockedRequest handler = this._requestExpectations.Peek();

            if (handler.Matches(request))
            {
                this._requestExpectations.Dequeue();

                return this.SendAsync(handler, request, cancellationToken);
            }
        }

        if (this._backendDefinitionBehavior == BackendDefinitionBehavior.Always
            || this._requestExpectations.Count == 0)
        {
            IMockedRequest? handler = this._backendDefinitions.Find(handler => handler.Matches(request));
            if (handler is not null)
            {
                return this.SendAsync(handler, request, cancellationToken);
            }
        }

        return this.SendAsync(Fallback, request, cancellationToken);
    }

    private Task<HttpResponseMessage> SendAsync(IMockedRequest handler,
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        Interlocked.Increment(ref this._outstandingRequests);

        this.IncrementMatchCount(handler);

        if (!this.AutoFlush)
        {
            this._flusher = new TaskCompletionSource<object>();
            this._pendingFlushers.Enqueue(this._flusher);
        }

        return this._flusher.Task.ContinueWith(_ =>
        {
            Interlocked.Decrement(ref this._outstandingRequests);

            cancellationToken.ThrowIfCancellationRequested();

            TaskCompletionSource<HttpResponseMessage> completionSource = new();

            cancellationToken.Register(() => completionSource.TrySetCanceled());

            handler.SendAsync(request, cancellationToken)
                .ContinueWith(resp =>
                {
                    resp.Result.RequestMessage = request;

                    if (resp.IsFaulted)
                    {
                        completionSource.TrySetException(resp.Exception ?? new Exception("Unexpected null exception"));
                    }
                    else if (resp.IsCanceled)
                    {
                        completionSource.TrySetCanceled();
                    }
                    else
                    {
                        completionSource.TrySetResult(resp.Result);
                    }
                }, cancellationToken);

            return completionSource.Task;
        }, cancellationToken).Unwrap();
    }

    private void IncrementMatchCount(IMockedRequest handler)
    {
        lock (this._lockObject)
        {
            this._matchCounts.TryGetValue(handler, out int count);
            this._matchCounts[handler] = count + 1;
        }
    }

    private async Task<HttpResponseMessage> CreateDefaultFallbackMessage(HttpRequestMessage req)
    {
        return await Task.Run(() =>
        {
            HttpResponseMessage message = new(HttpStatusCode.NotFound)
            {
                ReasonPhrase = $"No matching mock handler for \"{req.Method.ToString().ToUpperInvariant()} {req.RequestUri?.AbsoluteUri}\""
            };

            return message;
        });
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
        this._requestExpectations.Enqueue(handler);
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
        this._backendDefinitions.Add(handler);
    }

    /// <summary>
    /// Returns the number of times the specified request specification has been met
    /// </summary>
    /// <param name="request">The mocked request</param>
    /// <returns>The number of times the request has matched</returns>
    public int GetMatchCount(IMockedRequest request)
    {
        lock (this._lockObject)
        {
            this._matchCounts.TryGetValue(request, out int count);
            return count;
        }
    }

    /// <summary>
    /// Throws an <see cref="T:InvalidOperationException"/> if there are requests that were received 
    /// while <see cref="M:AutoFlush"/> was true, but have not been completed using <see cref="M:Flush"/>
    /// </summary>
    public void VerifyNoOutstandingRequest()
    {
        int requests = Interlocked.CompareExchange(ref _outstandingRequests, 0, 0);
        if (requests > 0)
        {
            throw new InvalidOperationException($"There are {requests} outstanding requests. Call Flush() to complete them");
        }
    }

    /// <summary>
    /// Throws an <see cref="T:InvalidOperationException"/> if there are any requests configured with Expects 
    /// that have yet to be received
    /// </summary>
    public void VerifyNoOutstandingExpectation()
    {
        if (this._requestExpectations.Count > 0)
        {
            throw new InvalidOperationException($"There are {this._requestExpectations.Count} unfulfilled expectations");
        }
    }

    /// <summary>
    /// Clears any pending requests configured with Expect
    /// </summary>
    public void ResetExpectations()
    {
        this._requestExpectations.Clear();
    }

    /// <summary>
    /// Clears any mocked requests configured with When
    /// </summary>
    public void ResetBackendDefinitions()
    {
        this._backendDefinitions.Clear();
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