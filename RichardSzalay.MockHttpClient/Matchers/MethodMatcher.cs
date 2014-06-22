using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;

namespace RichardSzalay.MockHttp.Matchers
{
    public class MethodMatcher: IMockedRequestMatcher
    {
        readonly HttpMethod method;

        public MethodMatcher(HttpMethod method)
        {
            this.method = method;
        }

        public bool Matches(HttpRequestMessage message)
        {
            return message.Method == this.method;
        }
    }
}
