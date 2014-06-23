using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RichardSzalay.MockHttp.Matchers
{
    /// <summary>
    /// A composite matcher that suceeds if any of it's composed matchers succeed
    /// </summary>
    public class AnyMatcher : IMockedRequestMatcher
    {
        readonly IEnumerable<IMockedRequestMatcher> _matchers;

        /// <summary>
        /// Construcuts a new instnace of AnyMatcher
        /// </summary>
        /// <param name="matchers">The list of matchers to evaluate</param>
        public AnyMatcher(IEnumerable<IMockedRequestMatcher> matchers)
        {
            this._matchers = matchers;
        }

        /// <summary>
        /// Determines whether the implementation matches a given request
        /// </summary>
        /// <param name="message">The request message being evaluated</param>
        /// <returns>true if any of the supplied matchers succeed; false otherwise</returns>
        public bool Matches(System.Net.Http.HttpRequestMessage message)
        {
            return _matchers.Any(m => m.Matches(message));
        }
    }
}
