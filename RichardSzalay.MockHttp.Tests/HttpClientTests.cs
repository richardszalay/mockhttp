using FluentAssertions;
using RichardSzalay.MockHttp.Extensions;

namespace RichardSzalay.MockHttp.Tests;

[TestClass]
public class HttpClientTests
{
    [TestInitialize]
    public void SetUp()
    {
    }

    [TestMethod]
    public async Task TestLogic()
    {
        MockHttpMessageHandler mockHttp = new MockHttpMessageHandler();

        // Setup a respond for the user api (including a wildcard in the URL)
        mockHttp.When("http://localhost/api/user/*")
            .Respond("application/json", "{'name' : 'Test McGee'}"); // Respond with JSON
        
        // Inject the handler or client into your application code
        var client = mockHttp.ToHttpClient();

        var response = await client.GetAsync("http://localhost/api/user/1234");

        var json = await response.Content.ReadAsStringAsync();

        json.Should().NotBeNullOrEmpty();
    }
}