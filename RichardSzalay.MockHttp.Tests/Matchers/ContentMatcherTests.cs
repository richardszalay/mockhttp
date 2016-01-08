using System.Net.Http;
using RichardSzalay.MockHttp.Matchers;
using Xunit;

namespace RichardSzalay.MockHttp.Tests.Matchers
{
    public class ContentMatcherTests
    {
        [Fact]
        public void Succeeds_on_matched_content()
        {
            var result = Test(
                expected: "Custom data",
                actual: "Custom data"
            );

            Assert.True(result);
        }

        [Fact]
        public void Fails_on_unmatched_content()
        {
            var result = Test(
                expected: "Custom data",
                actual: "Custom data!"
            );

            Assert.False(result);
        }

        private bool Test(string expected, string actual)
        {
            var request = new HttpRequestMessage(HttpMethod.Get,
                "http://tempuri.org/home");

            request.Content = new StringContent(actual);

            return new ContentMatcher(expected).Matches(request);
        }
    }
}
