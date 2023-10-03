using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace RichardSzalay.MockHttp.Tests.Issues;

public class Issue116Tests
{
    // https://github.com/richardszalay/mockhttp/issues/116
    [Fact]
    public async Task Can_simulate_timeout()
    {
        var handler = new MockHttpMessageHandler();

        handler.When("/topics/$aws%2Fthings%2Ftest-host%2Fshadow%2Fname%2Ftest-shadow%2Fupdate")
            .Respond(HttpStatusCode.OK);

        var client = new HttpClient(handler);

        var result = await client.GetAsync("http://localhost/topics/$aws%2Fthings%2Ftest-host%2Fshadow%2Fname%2Ftest-shadow%2Fupdate");
    }
}
