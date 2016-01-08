using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;

namespace RichardSzalay.MockHttp.Matchers
{
    /// <summary>
    /// Matches a request based on its request headers
    /// </summary>
    public class HeadersMatcher : IMockedRequestMatcher
    {
        readonly IEnumerable<KeyValuePair<string, string>> headers;

        /// <summary>
        /// Constructs a new instance of HeadersMatcher using a list of key value pairs to match
        /// </summary>
        /// <param name="headers">A list of key value pairs to match</param>
        public HeadersMatcher(IEnumerable<KeyValuePair<string, string>> headers)
        {
            this.headers = headers;
        }

        /// <summary>
        /// Constructs a new instance of HeadersMatcher using a formatted list of headers (Header: Value)
        /// </summary>
        /// <param name="headers">A formatted list of headers, separated by Environment.NewLine</param>
        public HeadersMatcher(string headers)
            : this(ParseHeaders(headers))
        {

        }

        /// <summary>
        /// Determines whether the implementation matches a given request
        /// </summary>
        /// <param name="message">The request message being evaluated</param>
        /// <returns>true if the request was matched; false otherwise</returns>
        public bool Matches(HttpRequestMessage message)
        {
            return this.headers.All(h => MatchesHeader(h, message.Headers));
        }

        private bool MatchesHeader(KeyValuePair<string, string> matchHeader, System.Net.Http.Headers.HttpRequestHeaders messageHeader)
        {
            IEnumerable<string> values;

            if (!messageHeader.TryGetValues(matchHeader.Key, out values))
                return false;

            return values.Any(v => v == matchHeader.Value);
        }

        internal static IEnumerable<KeyValuePair<string, string>> ParseHeaders(string headers)
        {
            List<KeyValuePair<string, string>> headerPairs = new List<KeyValuePair<string, string>>();

            using (StringReader reader = new StringReader(headers))
            {
                string line = reader.ReadLine();

                while (line != null)
                {
                    if (line.Trim().Length == 0)
                        break;

                    string[] parts = StringUtil.Split(line, ':', 2);

                    if (parts.Length != 2)
                        throw new ArgumentException("Invalid header: " + line);

                    headerPairs.Add(new KeyValuePair<string, string>(parts[0], parts[1].TrimStart(' ')));

                    line = reader.ReadLine();
                }
            }

            return headerPairs;
        }
    }
}
