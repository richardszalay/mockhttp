using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RichardSzalay.MockHttp.Matchers
{
    public class FormDataMatcher : IMockedRequestMatcher
    {
        private IEnumerable<KeyValuePair<string, string>> values;

        public FormDataMatcher(string formData)
            : this(QueryStringMatcher.ParseQueryString(formData))
        {
        }

        public FormDataMatcher(IEnumerable<KeyValuePair<string, string>> values)
        {
            this.values = values;
        }

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
