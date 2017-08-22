using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace RichardSzalay.MockHttp.Tests.Issues
{
    public class Issue33Tests
    {
        // https://github.com/richardszalay/mockhttp/issues/29
        [Fact]
        public async Task Disposing_response_does_not_fail_future_requests()
        {
            var handler = new MockHttpMessageHandler();

            handler.When("*").Respond(HttpStatusCode.OK);

            var client = new HttpClient(handler);

            var firstResponse = await client.GetAsync("http://localhost");
            firstResponse.Dispose();

            var secondResponse = await client.GetAsync("http://localhost");
        }
    }
}
