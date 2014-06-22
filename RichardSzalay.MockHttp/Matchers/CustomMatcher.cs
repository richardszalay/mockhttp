using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;

namespace RichardSzalay.MockHttp.Matchers
{
    /// <summary>
    /// Matches requests using a custom delegate
    /// </summary>
    public class CustomMatcher : IMockedRequestMatcher
    {
        readonly Func<HttpRequestMessage, bool> matcher;

        /// <summary>
        /// Constructs a new instance of CustomMatcher
        /// </summary>
        /// <param name="matcher">The matcher delegate</param>
        public CustomMatcher(Func<HttpRequestMessage, bool> matcher)
        {
            if (matcher == null)
                throw new ArgumentNullException("matcher");

            this.matcher = matcher;
        }

        /// <summary>
        /// Determines whether the implementation matches a given request
        /// </summary>
        /// <param name="message">The request message being evaluated</param>
        /// <returns>true if the request was matched; false otherwise</returns>
        public bool Matches(System.Net.Http.HttpRequestMessage message)
        {
            return matcher(message);
        }
    }
}
