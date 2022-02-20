using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
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

        [Fact]
        public void Description_contains_full_content()
		{
            const string expectedContent = "Custom data";
            var description = new ContentMatcher(expectedContent).Description;

            Assert.Contains(expectedContent, description);
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
