using RichardSzalay.MockHttp.Contracts;

namespace RichardSzalay.MockHttp.Matchers;

/// <summary>
/// Matches requests on form data values
/// </summary>
public class FormDataMatcher : IMockedRequestMatcher
{
    readonly IEnumerable<KeyValuePair<string, string>> values;
    readonly bool exact;

    /// <summary>
    /// Constructs a new instance of FormDataMatcher using a formatted query string
    /// </summary>
    /// <param name="formData">Formatted form data (key=value&amp;key2=value2)</param>
    /// <param name="exact">When true, requests with form data values not included in <paramref name="formData"/> will not match. Defaults to false</param>
    public FormDataMatcher(string formData, bool exact = false)
        : this(QueryStringMatcher.ParseQueryString(formData), exact)
    {
    }

    /// <summary>
    /// Constructs a new instance of FormDataMatcher using a list of key value pairs to match
    /// </summary>
    /// <param name="values">A list of key value pairs to match</param>
    /// <param name="exact">When true, requests with form data values not included in <paramref name="values"/> will not match. Defaults to false</param>
    public FormDataMatcher(IEnumerable<KeyValuePair<string, string>> values, bool exact = false)
    {
        this.values = values;
        this.exact = exact;
    }

    /// <summary>
    /// Determines whether the implementation matches a given request
    /// </summary>
    /// <param name="message">The request message being evaluated</param>
    /// <returns>true if the request was matched; false otherwise</returns>
    public bool Matches(System.Net.Http.HttpRequestMessage message)
    {
        if (!CanProcessContent(message.Content))
            return false;

        var formData = GetFormData(message.Content);

        var containsAllValues = values.All(matchPair =>
            formData.Any(p => p.Key == matchPair.Key && p.Value == matchPair.Value));

        if (!containsAllValues)
        {
            return false;
        }

        if (!exact)
        {
            return true;
        }

        return formData.All(matchPair =>
            values.Any(p => p.Key == matchPair.Key && p.Value == matchPair.Value));
    }

    private IEnumerable<KeyValuePair<string, string>> GetFormData(HttpContent content)
    {
        if (content is MultipartFormDataContent multipartContent)
        {
            return multipartContent
                .Where(CanProcessContent)
                .SelectMany(GetFormData);
        }

        string rawFormData = content.ReadAsStringAsync().Result;

        return QueryStringMatcher.ParseQueryString(rawFormData);
    }


    private bool CanProcessContent(HttpContent httpContent)
    {
        return httpContent != null &&
               httpContent.Headers.ContentType != null &&
               (IsFormData(httpContent.Headers.ContentType.MediaType) ||
                httpContent is MultipartFormDataContent);
    }

    private bool IsFormData(string mediaType)
    {
        return mediaType == "application/x-www-form-urlencoded";
    }
}