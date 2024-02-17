using RichardSzalay.MockHttp.Formatters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RichardSzalay.MockHttp.Matchers;

/// <summary>
/// Matches requests on querystring values
/// </summary>
public class QueryStringMatcher : IMockedRequestMatcher
{
    readonly IEnumerable<KeyValuePair<string, string>> values;
    readonly bool exact;
    readonly QueryStringMatcherOptions options;

    /// <summary>
    /// Constructs a new instance of QueryStringMatcher using a formatted query string
    /// </summary>
    /// <param name="queryString">A formatted query string (key=value&amp;key2=value2)</param>
    /// <param name="exact">When true, requests with querystring values not included in <paramref name="queryString"/> will not match. Defaults to false</param>
    /// <param name="options">The options used to match the querystring</param>
    public QueryStringMatcher(string queryString, bool exact = false, QueryStringMatcherOptions? options = null)
        : this(ParseQueryString(queryString), exact, options)
    {

    }

    /// <summary>
    /// Constructs a new instance of QueryStringMatcher using a list of key value pairs to match
    /// </summary>
    /// <param name="values">A list of key value pairs to match</param>
    /// <param name="exact">When true, requests with querystring values not included in <paramref name="values"/> will not match. Defaults to false</param>
    /// <param name="options">The options used to match the querystring. If null, defaults to ordinal case sensitive.</param>
    public QueryStringMatcher(IEnumerable<KeyValuePair<string, string>> values, bool exact = false, QueryStringMatcherOptions? options = null)
    {
        this.values = values;
        this.exact = exact;
        this.options = options ?? new QueryStringMatcherOptions();
    }

    /// <summary>
    /// Determines whether the implementation matches a given request
    /// </summary>
    /// <param name="message">The request message being evaluated</param>
    /// <returns>true if the request was matched; false otherwise</returns>
    public bool Matches(System.Net.Http.HttpRequestMessage message)
    {
        if (message.RequestUri == null)
        {
            return false;
        }

        var queryString = ParseQueryString(message.RequestUri.Query.TrimStart('?'));

        var keyComparer = options.KeyComparer;
        var valueComparer = options.ValueComparer;

        var containsAllValues = values.All(matchPair =>
            queryString.Any(p => keyComparer.Equals(p.Key, matchPair.Key) && valueComparer.Equals(p.Value, matchPair.Value)));

        if (!containsAllValues)
        {
            return false;
        }

        if (!exact)
        {
            return true;
        }

        return queryString.All(matchPair =>
            values.Any(p => keyComparer.Equals(p.Key, matchPair.Key) && valueComparer.Equals(p.Value, matchPair.Value)));
    }

    internal static IEnumerable<KeyValuePair<string, string>> ParseQueryString(string input)
    {
        return input.TrimStart('?').Split('&')
            .Select(pair => StringUtil.Split(pair, '=', 2))
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

    /// <inheritdoc/>
    public override string ToString()
    {
        var sb = new StringBuilder();

        var first = true;

        foreach (var kvp in this.values)
        {
            if (first)
            {
                first = false;
            }
            else
            {
                sb.Append('&');
            }

            sb.Append(Uri.EscapeDataString(kvp.Key));
            sb.Append('=');
            sb.Append(Uri.EscapeDataString(kvp.Value));
        }

        var resource = exact ? Resources.QueryStringMatcherDescriptor : Resources.PartialQueryStringMatcherDescriptor;
        return string.Format(resource, sb.ToString());
    }
}
