using RichardSzalay.MockHttp.Contracts;

namespace RichardSzalay.MockHttp.Matchers;

/// <summary>
/// Matches requests on querystring values
/// </summary>
public class QueryStringMatcher : IMockedRequestMatcher
{
    readonly IEnumerable<KeyValuePair<string, string>> values;
    readonly bool exact;

    /// <summary>
    /// Constructs a new instance of QueryStringMatcher using a formatted query string
    /// </summary>
    /// <param name="queryString">A formatted query string (key=value&amp;key2=value2)</param>
    /// <param name="exact">When true, requests with querystring values not included in <paramref name="queryString"/> will not match. Defaults to false</param>
    public QueryStringMatcher(string queryString, bool exact = false)
        : this(ParseQueryString(queryString), exact)
    {
    }

    /// <summary>
    /// Constructs a new instance of QueryStringMatcher using a list of key value pairs to match
    /// </summary>
    /// <param name="values">A list of key value pairs to match</param>
    /// <param name="exact">When true, requests with querystring values not included in <paramref name="values"/> will not match. Defaults to false</param>
    public QueryStringMatcher(IEnumerable<KeyValuePair<string, string>> values, bool exact = false)
    {
        this.values = values;
        this.exact = exact;
    }

    /// <summary>
    /// Determines whether the implementation matches a given request
    /// </summary>
    /// <param name="message">The request message being evaluated</param>
    /// <returns>true if the request was matched; false otherwise</returns>
    public bool Matches(HttpRequestMessage message)
    {
        var queryString = ParseQueryString(message.RequestUri?.Query.TrimStart('?'));

        var containsAllValues = values.All(matchPair =>
            queryString.Any(p => p.Key == matchPair.Key && p.Value == matchPair.Value));

        if (!containsAllValues)
        {
            return false;
        }

        if (!exact)
        {
            return true;
        }

        return queryString.All(matchPair =>
            values.Any(p => p.Key == matchPair.Key && p.Value == matchPair.Value));
    }

    internal static IEnumerable<KeyValuePair<string, string>> ParseQueryString(string? input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return Enumerable.Empty<KeyValuePair<string, string>>();
        }
        
        return input.TrimStart('?').Split('&')
            .Select(pair => pair.Split('=', 2))
            .Select(pair => new KeyValuePair<string, string>(
                UrlDecode(pair[0]),
                pair.Length == 2 ? UrlDecode(pair[1]) : ""
            ))
            .ToList();
    }

    internal static string UrlDecode(string urlEncodedValue)
    {
        string tmp = urlEncodedValue.Replace("+", "%20");
        return Uri.UnescapeDataString(tmp);
    }
}