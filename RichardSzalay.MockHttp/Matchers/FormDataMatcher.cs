using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
            if (message.Content == null ||
                message.Content.Headers.ContentType == null ||
                message.Content.Headers.ContentType.MediaType != "application/x-www-form-urlencoded")
            {
                return false;
            }

            // TODO: Add support for MultipartFormDataContent

            string rawFormData = message.Content.ReadAsStringAsync().Result;

            var formData = QueryStringMatcher.ParseQueryString(rawFormData);

            return values.All(matchPair =>
                formData.Any(p => p.Key == matchPair.Key && p.Value == matchPair.Value));
        }
    }
}
