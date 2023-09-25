using RichardSzalay.MockHttp.Contracts;

namespace RichardSzalay.MockHttp.Matchers;

/// <summary>
/// Matches requests on request content
/// </summary>
public class ContentMatcher : IMockedRequestMatcher
{
    private string content;

    /// <summary>
    /// Constructs a new instance of ContentMatcher
    /// </summary>
    /// <param name="content">The content to match</param>
    public ContentMatcher(string content)
    {
        this.content = content;
    }

    /// <summary>
    /// Determines whether the implementation matches a given request
    /// </summary>
    /// <param name="message">The request message being evaluated</param>
    /// <returns>true if the request was matched; false otherwise</returns>
    public bool Matches(System.Net.Http.HttpRequestMessage message)
    {
        if (message.Content == null)
            return false;

        string actualContent = message.Content.ReadAsStringAsync().Result;

        return actualContent == content;
    }
}