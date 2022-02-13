using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;

namespace RichardSzalay.MockHttp
{
    /// <summary>
    /// Represents a constraint on a mocked request
    /// </summary>
    public interface IMockedRequestMatcher
    {
        /// <summary>
        /// Determines whether the implementation matches a given request
        /// </summary>
        /// <param name="message">The request message being evaluated</param>
        /// <returns>true if the request was matched; false otherwise</returns>
        bool Matches(HttpRequestMessage message);

        /// <summary>
        /// A description of what will match this matcher
        /// </summary>
        string Description { get; }
    }
}
