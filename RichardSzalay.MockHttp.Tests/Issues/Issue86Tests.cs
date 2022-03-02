using System;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace RichardSzalay.MockHttp.Tests.Issues
{
    public class Issue86Tests
    {
        [Fact]
        public async Task Forgetting_to_configure_response_throws()
        {
            // Given
            var mockHttp = new MockHttp();
            mockHttp.Expect("/foo")
                .WithQueryString("bar", "bat")
                .WithQueryString("fizz", "buzz");
            var httpClient = mockHttp.ToHttpClient();

            // When
            var exception = Record.Exception(() => httpClient.GetAsync("/foo"));

            // Then
            Assert.NotNull(exception);
            Assert.IsType<InvalidOperationException>(exception);
            Assert.Equal("A response was not configured for this request", exception.message);
        }
    }
}
