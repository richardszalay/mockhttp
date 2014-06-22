using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RichardSzalay.MockHttp.Matchers
{
    public class PartialContentMatcher : IMockedRequestMatcher
    {
        private string content;

        public PartialContentMatcher(string content)
        {
            this.content = content;
        }

        public bool Matches(System.Net.Http.HttpRequestMessage message)
        {
            if (message.Content == null)
                return false;

            string actualContent = message.Content.ReadAsStringAsync().Result;

            return actualContent.IndexOf(content) != -1;
        }
    }
}
