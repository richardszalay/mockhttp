using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;

namespace RichardSzalay.MockHttp.Matchers
{
    public class UrlMatcher : IMockedRequestMatcher
    {
        readonly string url;

        public UrlMatcher(string url)
        {
            this.url = url;
        }

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
                ? url.AbsoluteUri
                : url.LocalPath;

            return source;
        }
    }
}
