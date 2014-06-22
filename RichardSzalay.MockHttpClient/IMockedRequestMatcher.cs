using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;

namespace RichardSzalay.MockHttp
{
    public interface IMockedRequestMatcher
    {
        bool Matches(HttpRequestMessage message);
    }
}
