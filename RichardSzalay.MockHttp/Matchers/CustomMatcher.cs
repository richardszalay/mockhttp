using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;

namespace RichardSzalay.MockHttp.Matchers
{
    public class CustomMatcher : IMockedRequestMatcher
    {
        readonly Func<HttpRequestMessage, bool> matcher;

        public CustomMatcher(Func<HttpRequestMessage, bool> matcher)
        {
            if (matcher == null)
                throw new ArgumentNullException("matcher");

            this.matcher = matcher;
        }

        public bool Matches(System.Net.Http.HttpRequestMessage message)
        {
            return matcher(message);
        }
    }
}
