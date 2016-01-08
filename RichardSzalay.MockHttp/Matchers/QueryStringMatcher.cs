using System;
using System.Collections.Generic;
using System.Linq;

namespace RichardSzalay.MockHttp.Matchers
{
    /// <summary>
    /// Matches requests on querystring values
    /// </summary>
    public class QueryStringMatcher : IMockedRequestMatcher
    {
        readonly IEnumerable<KeyValuePair<string, string>> values;

        /// <summary>
        /// Constructs a new instance of QueryStringMatcher using a formatted query string
        /// </summary>
        /// <param name="queryString">A formatted query string (key=value&amp;key2=value2)</param>
        public QueryStringMatcher(string queryString)
            : this(ParseQueryString(queryString))
        {

        }

        /// <summary>
        /// Constructs a new instance of QueryStringMatcher using a list of key value pairs to match
        /// </summary>
        /// <param name="values">A list of key value pairs to match</param>
        public QueryStringMatcher(IEnumerable<KeyValuePair<string, string>> values)
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
            var queryString = ParseQueryString(message.RequestUri.Query.TrimStart('?'));

            return values.All(matchPair => 
                queryString.Any(p => p.Key == matchPair.Key && p.Value == matchPair.Value));
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
    }
}
