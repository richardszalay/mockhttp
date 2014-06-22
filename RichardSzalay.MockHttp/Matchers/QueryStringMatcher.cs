using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RichardSzalay.MockHttp.Matchers
{
    public class QueryStringMatcher : IMockedRequestMatcher
    {
        readonly IEnumerable<KeyValuePair<string, string>> values;

        public QueryStringMatcher(string queryString)
            : this(ParseQueryString(queryString))
        {

        }

        public QueryStringMatcher(IEnumerable<KeyValuePair<string, string>> values)
        {
            this.values = values;
        }

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
                    Uri.UnescapeDataString(pair[0]),
                    pair.Length == 2 ? Uri.UnescapeDataString(pair[1]) : null
                    ))
                .ToList();
        }
    }
}
