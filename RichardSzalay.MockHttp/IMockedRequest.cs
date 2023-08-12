using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace RichardSzalay.MockHttp;

/// <summary>
/// A preconfigured response to a HTTP request
/// </summary>
public interface IMockedRequest
{
    /// <summary>
    /// Determines if a request can be handled by this instance
    /// </summary>
    /// <param name="request">The <see cref="T:HttpRequestMessage"/> being sent</param>
    /// <returns>true if this instance can handle the request; false otherwise</returns>
    bool Matches(HttpRequestMessage request);

    /// <summary>
    /// Submits the request to be handled by this instance
    /// </summary>
    /// <param name="message">The request message being sent</param>
    /// <param name="cancellationToken">A <see cref="T:CancellationToken"/> for long running requests</param>
    /// <returns>The <see cref="T:HttpResponseMessage"/> to the request</returns>
    Task<HttpResponseMessage> SendAsync(HttpRequestMessage message, CancellationToken cancellationToken);
}
