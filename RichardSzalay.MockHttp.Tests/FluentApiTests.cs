using System.Net.Http;
using Xunit;

namespace RichardSzalay.MockHttp.Tests
{
    public class FluentApiTests
    {
        [Fact]
        public void Fluent_Api_can_be_used_to_simplify_syntax()
        {
            // Arrange
            var httpClient = new MockHttpMessageHandler()
                .When("http://localhost/a").Respond("text/plain", "result-a")
                .When("http://localhost/b").Respond("text/plain", "result-b")
                .AsHttpClient();

            var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/a");

            // Act
            string result = httpClient.SendAsync(request).Result.Content.ReadAsStringAsync().Result;

            // Assert
            Assert.Equal("result-a", result);
        }
    }
}
