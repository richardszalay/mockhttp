using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using RichardSzalay.MockHttp.Matchers;

namespace RichardSzalay.MockHttp
{
    public static class MockHttpMessageHandlerExtensions
    {
        /// <summary>
        /// Adds a backend definition 
        /// </summary>
        /// <param name="method">The HTTP method to match</param>
        /// <param name="url">The URL (absolute or relative, may contain * wildcards) to match</param>
        /// <returns>The <see cref="T:MockedRequest"/> instance</returns>
        public static MockedRequest When(this MockHttpMessageHandler handler, HttpMethod method, string url)
        {
            var message = new MockedRequest(url);
            message.With(new MethodMatcher(method));

            handler.AddBackendDefinition(message);

            return message;
        }

        /// <summary>
        /// Adds a backend definition 
        /// </summary>
        /// <param name="url">The URL (absolute or relative, may contain * wildcards) to match</param>
        /// <returns>The <see cref="T:MockedRequest"/> instance</returns>
        public static MockedRequest When(this MockHttpMessageHandler handler, string url)
        {
            var message = new MockedRequest(url);

            handler.AddBackendDefinition(message);

            return message;
        }

        /// <summary>
        /// Adds a request expectation
        /// </summary>
        /// <param name="method">The HTTP method to match</param>
        /// <param name="url">The URL (absolute or relative, may contain * wildcards) to match</param>
        /// <returns>The <see cref="T:MockedRequest"/> instance</returns>
        public static MockedRequest Expect(this MockHttpMessageHandler handler, HttpMethod method, string url)
        {
            var message = new MockedRequest(url);
            message.With(new MethodMatcher(method));

            handler.AddRequestExpectation(message);

            return message;
        }

        /// <summary>
        /// Adds a request expectation
        /// </summary>
        /// <param name="url">The URL (absolute or relative, may contain * wildcards) to match</param>
        /// <returns>The <see cref="T:MockedRequest"/> instance</returns>
        public static MockedRequest Expect(this MockHttpMessageHandler handler, string url)
        {
            var message = new MockedRequest(url);

            handler.AddRequestExpectation(message);

            return message;
        }
    }
}
