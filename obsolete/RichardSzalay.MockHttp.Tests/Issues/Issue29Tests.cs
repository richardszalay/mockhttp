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
    public class Issue29Tests
    {
        // https://github.com/richardszalay/mockhttp/issues/29
        [Fact]
        public async Task Can_simulate_timeout()
        {
            var handler = new MockHttpMessageHandler();

            handler.Fallback.Respond(async () =>
            {
                await Task.Delay(10000);

                return new HttpResponseMessage(HttpStatusCode.OK);
            });

            var client = new HttpClient(handler);
            client.Timeout = TimeSpan.FromMilliseconds(1000);

            try
            {
                var result = await client.GetAsync("http://localhost");

                throw new InvalidOperationException("Expected timeout exception");
            }
            catch(OperationCanceledException)
            {
            }
        }
    }
}
