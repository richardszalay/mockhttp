using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace RichardSzalay.MockHttp.Tests.Issues;

public class Issue149Tests
{
    private HttpClient client;

    //https://github.com/richardszalay/mockhttp/issues/149
    [Fact]
    public async Task MultipleJsonContentMatchersToSameUri()
    {
        var mockHttp = new MockHttpMessageHandler();
        mockHttp
            .When("https://someUrl.com")
            .WithJsonContent<SomeJsonParameters>(a => a.Key == "SomeApiKey")
            .Respond("application/json", "{\"some_result\" : \"Error\"}");
        mockHttp
            .When("https://someUrl.com")
            .WithJsonContent<SomeJsonParameters>(a => a.Content == "SomeContent")
            .Respond("application/json", "{\"some_result\" : \"Result\"}");

        client = mockHttp.ToHttpClient();

        var parameters = JsonSerializer.Serialize(new SomeJsonParameters("SomeApiKey", "OtherContent"));
        var response = await client.PostAsync(
            "https://someUrl.com",
            new StringContent(parameters, Encoding.UTF8, "application/json"));
        var content = await response.Content.ReadAsStringAsync();
        Assert.True(!string.IsNullOrEmpty(content));

        var parameters1 = JsonSerializer.Serialize(new SomeJsonParameters("OtherApiKey", "SomeContent"));
        var action1 = async ()=> await client.PostAsync(
            "https://someUrl.com",
            new StringContent(parameters1, Encoding.UTF8, "application/json"));
        var exception = await Record.ExceptionAsync(action1);
        Assert.Null(exception);
    }

    private record SomeJsonParameters(string Key, string Content);
}