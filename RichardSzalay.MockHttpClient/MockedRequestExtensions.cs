using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using RichardSzalay.MockHttp.Matchers;

namespace RichardSzalay.MockHttp
{
    public static class MockedRequestExtensions
    {
        /// <summary>
        /// Constraints the request using custom logic
        /// </summary>
        /// <param name="matcher">The delegate that will be used to constrain the request</param>
        /// <returns>The <see cref="T:MockedRequest"/> instance</returns>
        public static MockedRequest With(this MockedRequest source, Func<HttpRequestMessage, bool> matcher)
        {
            return source.With(new CustomMatcher(matcher));
        }

        /// <summary>
        /// Includes requests contain a particular query string value
        /// </summary>
        /// <param name="name">The query string key to match</param>
        /// <param name="value">The query string value to match (including null)</param>
        /// <returns>The <see cref="T:MockedRequest"/> instance</returns>
        public static MockedRequest WithQueryString(this MockedRequest source, string name, string value)
        {
            return WithHeaders(source, new Dictionary<string, string>
            {
                { name, value }
            });
        }

        /// <summary>
        /// Includes requests contain a set of query string values
        /// </summary>
        /// <param name="values">The query string key/value pairs to match</param>
        /// <returns>The <see cref="T:MockedRequest"/> instance</returns>
        public static MockedRequest WithQueryString(this MockedRequest source, IEnumerable<KeyValuePair<string, string>> values)
        {
            source.With(new HeadersMatcher(values));

            return source;
        }

        /// <summary>
        /// Includes requests contain a set of query string values
        /// </summary>
        /// <param name="values">A formatted query string containing key/value pairs to match</param>
        /// <returns>The <see cref="T:MockedRequest"/> instance</returns>
        public static MockedRequest WithQueryString(this MockedRequest source, string headers)
        {
            source.With(new HeadersMatcher(headers));

            return source;
        }

        /// <summary>
        /// Includes requests contain a particular header
        /// </summary>
        /// <param name="name">The HTTP header name</param>
        /// <param name="value">The value of the HTTP header to match</param>
        /// <returns>The <see cref="T:MockedRequest"/> instance</returns>
        public static MockedRequest WithHeaders(this MockedRequest source, string name, string value)
        {
            return WithHeaders(source, new Dictionary<string, string>
            {
                { name, value }
            });
        }

        /// <summary>
        /// Includes requests contain a set of headers
        /// </summary>
        /// <param name="headers">A list of HTTP header name/value pairs</param>
        /// <returns>The <see cref="T:MockedRequest"/> instance</returns>
        public static MockedRequest WithHeaders(this MockedRequest source, IEnumerable<KeyValuePair<string, string>> headers)
        {
            source.With(new HeadersMatcher(headers));

            return source;
        }

        /// <summary>
        /// Includes requests contain a set of headers
        /// </summary>
        /// <param name="headers">A string containing headers as they would appear in the HTTP request</param>
        /// <returns>The <see cref="T:MockedRequest"/> instance</returns>
        public static MockedRequest WithHeaders(this MockedRequest source, string headers)
        {
            source.With(new HeadersMatcher(headers));

            return source;
        }

        /// <summary>
        /// Sets the response of the current <see cref="T:MockedRequest"/>
        /// </summary>
        /// <param name="message">The complete <see cref="T:HttpResponseMessage"/> to return</param>
        public static void Respond(this MockedRequest source, HttpResponseMessage message)
        {
            source.Respond(_ => TaskEx.FromResult(message));
        }

        /// <summary>
        /// Sets the response of the current <see cref="T:MockedRequest"/>
        /// </summary>
        /// <param name="statusCode">The <see cref="T:HttpStatusCode"/> of the response</param>
        public static void Respond(this MockedRequest source, HttpStatusCode statusCode)
        {
            source.Respond(new HttpResponseMessage(statusCode));
        }

        /// <summary>
        /// Sets the response of the current <see cref="T:MockedRequest"/>
        /// </summary>
        /// <param name="statusCode">The <see cref="T:HttpStatusCode"/> of the response</param>
        /// <param name="content">The content of the response</param>
        public static void Respond(this MockedRequest source, HttpStatusCode statusCode, HttpContent content)
        {
            HttpResponseMessage message = new HttpResponseMessage(statusCode);
            message.Content = content;

            source.Respond(message);
        }

        /// <summary>
        /// Sets the response of the current <see cref="T:MockedRequest"/>, with an OK (200) status code
        /// </summary>
        /// <param name="content">The content of the response</param>
        public static void Respond(this MockedRequest source, HttpContent content)
        {
            source.Respond(HttpStatusCode.OK, content);
        }

        /// <summary>
        /// Sets the response of the current <see cref="T:MockedRequest"/>
        /// </summary>
        /// <param name="statusCode">The <see cref="T:HttpStatusCode"/> of the response</param>
        /// <param name="content">The content of the response</param>
        /// <param name="mediaType">The media type of the response</param>
        public static void Respond(this MockedRequest source, HttpStatusCode statusCode, string mediaType, string content)
        {
            source.Respond(statusCode, new StringContent(content, Encoding.UTF8, mediaType));
        }

        /// <summary>
        /// Sets the response of the current <see cref="T:MockedRequest"/>, with an OK (200) status code
        /// </summary>
        /// <param name="content">The content of the response</param>
        /// <param name="mediaType">The media type of the response</param>
        public static void Respond(this MockedRequest source, string mediaType, string content)
        {
            source.Respond(HttpStatusCode.OK, mediaType, content);
        }

        /// <summary>
        /// Sets the response of the current <see cref="T:MockedRequest"/>
        /// </summary>
        /// <param name="statusCode">The <see cref="T:HttpStatusCode"/> of the response</param>
        /// <param name="content">The content of the response</param>
        /// <param name="mediaType">The media type of the response</param>
        public static void Respond(this MockedRequest source, HttpStatusCode statusCode, string mediaType, Stream content)
        {
            var streamContent = new StreamContent(content);
            streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(mediaType);
            
            source.Respond(statusCode, streamContent);
        }

        /// <summary>
        /// Sets the response of the current <see cref="T:MockedRequest"/>, with an OK (200) status code
        /// </summary>
        /// <param name="content">The content of the response</param>
        /// <param name="mediaType">The media type of the response</param>
        public static void Respond(this MockedRequest source, string mediaType, Stream content)
        {
            source.Respond(HttpStatusCode.OK, mediaType, content);
        }

        /// <summary>
        /// Sets the response of the current <see cref="T:MockedRequest"/>
        /// </summary>
        /// <param name="handler">The delegate that will return a <see cref="T:HttpResponseMessage"/> determined at runtime</param>
        public static void Respond(this MockedRequest source, Func<HttpRequestMessage, HttpResponseMessage> handler)
        {
            source.Respond(req => handler(req));
        }
    }
}
