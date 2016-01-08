using System;
using System.Net.Http;

namespace RichardSzalay.MockHttp.Matchers
{
    /// <summary>
    /// Matches requests on their URL
    /// </summary>
    public class UrlMatcher : IMockedRequestMatcher
    {
        readonly string url;

        /// <summary>
        /// Constructs a new instance of UrlMatcher
        /// </summary>
        /// <param name="url">The url (relative or absolute) to match</param>
        public UrlMatcher(string url)
        {
            Uri uri;
            if (Uri.TryCreate(url, UriKind.Absolute, out uri))
                url = uri.AbsoluteUri;

            this.url = url;
        }

        /// <summary>
        /// Determines whether the implementation matches a given request
        /// </summary>
        /// <param name="message">The request message being evaluated</param>
        /// <returns>true if the request was matched; false otherwise</returns>
        public bool Matches(HttpRequestMessage message)
        {
            if (String.IsNullOrEmpty(url) || url == "*")
                return true;

            string matchUrl = GetUrlToMatch(message.RequestUri);

            bool startsWithWildcard = url.StartsWith("*");
            bool endsWithWildcard = url.EndsWith("*");

            string[] matchParts = url.Split(new [] { '*' }, StringSplitOptions.RemoveEmptyEntries);

            if (matchParts.Length == 0)
                return true;

            if (!startsWithWildcard)
            {
                if (!matchUrl.StartsWith(matchParts[0]))
                    return false;
            }

            int position = 0;

            foreach(var matchPart in matchParts)
            {
                position = matchUrl.IndexOf(matchPart, position);

                if (position == -1)
                    return false;

                position += matchPart.Length;
            }

            if (!endsWithWildcard && position != matchUrl.Length)
            {
                return false;
            }

            return true;
        }

        private string GetUrlToMatch(Uri url)
        {
            bool matchingFullUrl = Uri.IsWellFormedUriString(this.url.Replace('*', '-'), UriKind.Absolute);

            string source = matchingFullUrl
                ? new UriBuilder(url) {  Query = "" }.Uri.AbsoluteUri
                : url.LocalPath;

            return source;
        }
    }
}
