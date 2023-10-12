using RichardSzalay.MockHttp.Matchers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace RichardSzalay.MockHttp;

/// <summary>
/// Responds to requests using pre-configured responses
/// </summary>
public class MockHttpMessageHandler : HttpMessageHandler
{
    private Queue<IMockedRequest> requestExpectations = new();
    private List<IMockedRequest> backendDefinitions = new();
    private Dictionary<IMockedRequest, int> matchCounts = new();
    private object lockObject = new();

    private int outstandingRequests = 0;

    /// <summary>
    /// Creates a new instance of MockHttpMessageHandler
    /// </summary>
    public MockHttpMessageHandler(BackendDefinitionBehavior backendDefinitionBehavior = BackendDefinitionBehavior.NoExpectations)
    {
        this.backendDefinitionBehavior = backendDefinitionBehavior;

        AutoFlush = true;
        fallback = new MockedRequest();
        fallback.ThrowMatchSummary();
    }

    private bool autoFlush;
    readonly BackendDefinitionBehavior backendDefinitionBehavior;

    /// <summary>
    /// Requests received while AutoFlush is true will complete instantly. 
    /// Requests received while AutoFlush is false will not complete until <see cref="M:Flush"/> is called
    /// </summary>
    public bool AutoFlush
    {
        get => autoFlush;
        set
        {
            autoFlush = value;

            if (autoFlush)
            {
                flusher = new TaskCompletionSource<object?>();
                flusher.SetResult(null);
            }
            else
            {
                flusher = new TaskCompletionSource<object?>();
                pendingFlushers.Enqueue(flusher);
            }
        }
    }

    private Queue<TaskCompletionSource<object?>> pendingFlushers = new();
    private TaskCompletionSource<object?> flusher = default!; // Assigned in ctor via AutoFlush setter

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
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var handler = FindHandler(request);

        return SendAsync(handler, request, cancellationToken);
    }

    private IMockedRequest FindHandler(HttpRequestMessage request)
    {
        var results = new RequestHandlerResult(request);

        if (requestExpectations.Count > 0)
        {
            var handler = requestExpectations.Peek();
            var handlerResult = EvaluateMockedRequest(handler, request);
            results.RequestExpectationResult = handlerResult;

            results.UnevaluatedRequestExpectations.AddRange(requestExpectations.Skip(1));

            if (handlerResult.Success)
            {
                requestExpectations.Dequeue();

                results.Handler = handler;
            }
        }

        var evaluateBackendDefinitions = backendDefinitionBehavior == BackendDefinitionBehavior.Always
            || requestExpectations.Count == 0;

        foreach (var handler in backendDefinitions)
        {
            var evaludateHandler = results.Handler == null && evaluateBackendDefinitions;

            if (!evaludateHandler)
            {
                results.UnevaluatedBackendDefinitions.Add(handler);
                continue;
            }

            var handlerResult = EvaluateMockedRequest(handler, request);
            results.BackendDefinitionResults.Add(handlerResult);

            if (handlerResult.Success)
            {
                results.Handler = handler;
            }
        }

        SetHandlerResult(request, results);

        return results.Handler ?? Fallback;
    }

    private MockedRequestResult EvaluateMockedRequest(IMockedRequest mockedRequest, HttpRequestMessage request)
    {
        Dictionary<IMockedRequestMatcher, bool> matcherResults = new();

        // Once a decision around public API changes is made, the new API can have matchers
        // return a richer object, removing the need for explicit knowledge of 'AnyMatcher' here
        bool IsAnyMatch(AnyMatcher matcher)
        {
            foreach (var childMatcher in matcher)
            {
                var childResult = childMatcher.Matches(request);

                matcherResults[childMatcher] = childResult;

                if (childResult)
                {
                    return true;
                }
            }

            return false;
        }

        // This is an odd way of achieving this but allows the model to developed/iterated without changing
        // the public API (for now)
        if (mockedRequest is not IEnumerable<IMockedRequestMatcher> matchers)
        {
            return MockedRequestResult.FromResult(mockedRequest,
                mockedRequest.Matches(request));
        }

        var success = true;

        foreach (var matcher in matchers)
        {
            var matcherResult = matcher switch
            {
                AnyMatcher anyMatcher => IsAnyMatch(anyMatcher),
                _ => matcher.Matches(request)
            };
            matcherResults[matcher] = matcherResult;

            if (!matcherResult)
            {
                success = false;
                break;
            }
        }

        return MockedRequestResult.FromMatcherResults(mockedRequest, matcherResults, success);
    }

#if NET5_0_OR_GREATER
    /// <summary>
    /// Maps the request to the most appropriate configured response
    /// </summary>
    /// <param name="request">The request being sent</param>
    /// <param name="cancellationToken">The token used to cancel the request</param>
    /// <returns>A Task containing the future response message</returns>
    protected override HttpResponseMessage Send(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // TODO: Throw is AutoFlush is disabled

        return SendAsync(request, cancellationToken)
            .GetAwaiter().GetResult();
    }
#endif

    private Task<HttpResponseMessage> SendAsync(IMockedRequest handler, HttpRequestMessage request, CancellationToken cancellationToken)
    {
        Interlocked.Increment(ref outstandingRequests);

        IncrementMatchCount(handler);

        if (!AutoFlush)
        {
            flusher = new TaskCompletionSource<object?>();
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
                            completionSource.TrySetException(resp.Exception!);
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
        lock (lockObject)
        {
            matchCounts.TryGetValue(handler, out int count);
            matchCounts[handler] = count + 1;
        }
    }

    private MockedRequest fallback;

    /// <summary>
    /// Gets the <see cref="T:MockedRequest"/> that will handle requests that were otherwise unmatched
    /// </summary>
    public MockedRequest Fallback
    {
        get => fallback;
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
        lock (lockObject)
        {
            matchCounts.TryGetValue(request, out int count);
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
            throw new InvalidOperationException("There are " + requests + " outstanding requests. Call Flush() to complete them");
    }

    /// <summary>
    /// Throws an <see cref="T:InvalidOperationException"/> if there are any requests configured with Expects 
    /// that have yet to be received
    /// </summary>
    public void VerifyNoOutstandingExpectation()
    {
        if (requestExpectations.Count > 0)
            throw new InvalidOperationException("There are " + requestExpectations.Count + " unfulfilled expectations");
    }

    /// <summary>
    /// Clears any pending requests configured with Expect
    /// </summary>
    public void ResetExpectations()
    {
        requestExpectations.Clear();
    }

    /// <summary>
    /// Clears any mocked requests configured with When
    /// </summary>
    public void ResetBackendDefinitions()
    {
        backendDefinitions.Clear();
    }

    /// <summary>
    /// Clears all mocked requests configured with either Expect or When
    /// </summary>
    public void Clear()
    {
        ResetExpectations();
        ResetBackendDefinitions();
    }

#pragma warning disable CS0618 // Type or member is obsolete. We need Properties as Options isn't supported < .NET 5
    private const string HandlerResultMessageKey = "_MockHttpResult";

    internal static RequestHandlerResult? GetHandlerResult(HttpRequestMessage request)
    {

        if (request.Properties.TryGetValue(HandlerResultMessageKey, out var result) == true &&
            result is RequestHandlerResult handlerResult)
        {
            return handlerResult;
        }

        return null;
    }

    internal static void SetHandlerResult(HttpRequestMessage request, RequestHandlerResult handlerResult)
    {

        request.Properties[HandlerResultMessageKey] = handlerResult;
    }

#pragma warning restore CS0618 // Type or member is obsolete
}
