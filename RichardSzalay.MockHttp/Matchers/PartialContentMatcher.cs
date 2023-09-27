using RichardSzalay.MockHttp.Contracts;

namespace RichardSzalay.MockHttp.Matchers;

/// <summary>
/// Matches requests on partial request content
/// </summary>
public class PartialContentMatcher : IMockedRequestMatcher
{
    private readonly string _content;

    /// <summary>
    /// Constructs a new instance of PartialContentMatcher
    /// </summary>
    /// <param name="content">The partial content to match</param>
    public PartialContentMatcher(string content)
    {
        this._content = content;
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

        return actualContent.IndexOf(_content, StringComparison.Ordinal) != -1;
    }
}