using RichardSzalay.MockHttp.Matchers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace RichardSzalay.MockHttp;

/// <summary>
/// A preconfigured response to a HTTP request
/// </summary>
public class MockedRequest : IMockedRequest, IEnumerable<IMockedRequestMatcher>
{
    private List<IMockedRequestMatcher> matchers = new List<IMockedRequestMatcher>();
    private Func<HttpRequestMessage, Task<HttpResponseMessage>>? response;

    /// <summary>
    /// Creates a new MockedRequest with no initial matchers
    /// </summary>
    public MockedRequest()
    {
    }

    /// <summary>
    /// Creates a new MockedRequest with an initial URL (and optionally query string) matcher
    /// </summary>
    /// <param name="url">An absolute or relative URL that may contain a query string</param>
    public MockedRequest(string url)
    {
        string[] urlParts = StringUtil.Split(url, '?', 2);

        if (urlParts.Length == 2)
            url = urlParts[0];

        if (urlParts.Length == 2)
            With(new QueryStringMatcher(urlParts[1]));

        With(new UrlMatcher(url));
    }

    /// <summary>
    /// Determines if a request can be handled by this instance
    /// </summary>
    /// <param name="message">The <see cref="T:HttpRequestMessage"/> being sent</param>
    /// <returns>true if this instance can handle the request; false otherwise</returns>
    public bool Matches(HttpRequestMessage message)
    {
        return matchers.Count == 0 || matchers.All(m => m.Matches(message));
    }

    /// <summary>
    /// Constraints the request using custom logic
    /// </summary>
    /// <returns>The <see cref="T:MockedRequest"/> instance</returns>
    public MockedRequest With(IMockedRequestMatcher matcher)
    {
        matchers.Add(matcher);
        return this;
    }

    /// <summary>
    /// Sets the response of the request
    /// </summary>
    /// <param name="handler"></param>
    public void Respond(Func<Task<HttpResponseMessage>> handler)
    {
        Respond(_ => handler());
    }

    /// <summary>
    /// Supplies a response to the submitted request
    /// </summary>
    /// <param name="handler">The callback that will be used to supply the response</param>
    public MockedRequest Respond(Func<HttpRequestMessage, Task<HttpResponseMessage>> handler)
    {
        response = handler;
        return this;
    }

    /// <summary>
    /// Provides the configured response in relation to the request supplied
    /// </summary>
    /// <param name="message">The request being sent</param>
    /// <param name="cancellationToken">The token used to cancel the request</param>
    /// <exception cref="InvalidOperationException">A response was not configured for this request</exception>
    /// <returns>A Task containing the future response message</returns>
    public Task<HttpResponseMessage> SendAsync(HttpRequestMessage message, CancellationToken cancellationToken)
    {
        if (response is null)
        {
            throw new InvalidOperationException("A response was not configured for this request");
        }

        return response(message);
    }

    IEnumerator<IMockedRequestMatcher> IEnumerable<IMockedRequestMatcher>.GetEnumerator()
        => this.matchers.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => this.matchers.GetEnumerator();
}
