using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace RichardSzalay.MockHttp.Matchers
{
    /// <summary>
    /// Matches requests on form data values
    /// </summary>
    public class FormDataMatcher : IMockedRequestMatcher
    {
        private IEnumerable<KeyValuePair<string, string>> values;

        /// <summary>
        /// Constructs a new instance of FormDataMatcher using a formatted query string
        /// </summary>
        /// <param name="formData">Formatted form data (key=value&amp;key2=value2)</param>
        public FormDataMatcher(string formData)
            : this(QueryStringMatcher.ParseQueryString(formData))
        {
        }

        /// <summary>
        /// Constructs a new instance of FormDataMatcher using a list of key value pairs to match
        /// </summary>
        /// <param name="values">A list of key value pairs to match</param>
        public FormDataMatcher(IEnumerable<KeyValuePair<string, string>> values)
        {
            this.values = values;
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

            return values.All(matchPair =>
                formData.Any(p => p.Key == matchPair.Key && p.Value == matchPair.Value));
        }

        private IEnumerable<KeyValuePair<string, string>> GetFormData(HttpContent content)
        {
            if (content is MultipartFormDataContent)
            {
                return ((MultipartFormDataContent)content)
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
}
