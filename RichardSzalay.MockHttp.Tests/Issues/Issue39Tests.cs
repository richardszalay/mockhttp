using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace RichardSzalay.MockHttp.Tests.Issues
{
    public class Issue39Tests
    {
        [Fact]
        public async Task Responding_with_HttpClient_Works()
        {
            var mockSecondHandler = new MockHttpMessageHandler();
            mockSecondHandler.When("*").Respond(HttpStatusCode.OK);

            var mockFirstHandler = new MockHttpMessageHandler();
            mockFirstHandler.When("*").Respond(mockSecondHandler.ToHttpClient());

            var response = await mockFirstHandler.ToHttpClient().GetAsync("http://localhost/");

            Assert.Equal(response.StatusCode, HttpStatusCode.OK);
        }
    }
}
