using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RichardSzalay.MockHttp.Matchers;

namespace RichardSzalay.MockHttp
{
    /// <summary>
    /// A preconfigured response to a HTTP request
    /// </summary>
    public class MockedRequest : IMockedRequest
    {
        private List<IMockedRequestMatcher> matchers = new List<IMockedRequestMatcher>();
        private Func<HttpRequestMessage, Task<HttpResponseMessage>> response;

        public MockedRequest()
        {
        }

        public MockedRequest(string url)
        {
            string[] urlParts = url.Split(new[] { '?' }, 2);

            if (urlParts.Length == 2)
                url = urlParts[0];

            if (urlParts.Length == 2)
                this.With(new QueryStringMatcher(urlParts[1]));

            this.With(new UrlMatcher(url));
        }

        /// <summary>
        /// Determines if a request can be handled by this instance
        /// </summary>
        /// <param name="request">The <see cref="T:HttpRequestMessage"/> being sent</param>
        /// <returns>true if this instance can handle the request; false otherwise</returns>
        public bool Matches(HttpRequestMessage message)
        {
            return matchers.Count == 0 || matchers.All(m => m.Matches(message));
        }

        /// <summary>
        /// Constraints the request using custom logic
        /// </summary>
        /// <returns>The <see cref="T:MockedRequest"/> instance</returns>
        public MockedRequest With(IMockedRequestMatcher matcher)
        {
            this.matchers.Add(matcher);
            return this;
        }

        /// <summary>
        /// Sets the response of ther 
        /// </summary>
        /// <param name="handler"></param>
        public void Respond(Func<Task<HttpResponseMessage>> handler)
        {
            this.Respond(_ => handler());
        }

        public void Respond(Func<HttpRequestMessage, Task<HttpResponseMessage>> handler)
        {
            this.response = handler;
        }

        public Task<HttpResponseMessage> SendAsync(HttpRequestMessage message, CancellationToken cancellationToken)
        {
            return response(message);
        }
    }
}
