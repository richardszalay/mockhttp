using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using RichardSzalay.MockHttp.Matchers;
using Xunit;

namespace RichardSzalay.MockHttp.Tests.Matchers
{
    public class HeadersMatcherTests
    {
        [Fact]
        public void Should_succeed_on_all_matched()
        {
            bool result = Test(
                expected: new HeadersMatcher(new Dictionary<string, string>
                    {
                        { "Authorization", "Basic abcdef" },
                        { "Accept", "application/json" },
                        { "Content-Type", "text/plain; charset=utf-8" }
                    }),
                actual: req =>
                {
                    req.Headers.Authorization = new AuthenticationHeaderValue("Basic", "abcdef");
                    req.Headers.Accept.Clear();
                    req.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    req.Content = new StringContent("test", Encoding.UTF8, "text/plain");
                }
                );

            Assert.True(result);
        }

        [Fact]
        public void Should_parse_string_headers()
        {
            bool result = Test(
                expected: new HeadersMatcher(@"Accept: application/json
Authorization: Basic abcdef"),
                actual: req =>
                    {
                        req.Headers.Authorization = new AuthenticationHeaderValue("Basic", "abcdef");
                        req.Headers.Accept.Clear();
                        req.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    }
                );

            Assert.True(result);
        }

        [Fact]
        public void Description_shows_expected_string_headers()
        {
	        var authorizationHeader = "Authorization: Bearer my_token";
	        var contentTypeHeader = "Content-Type: application/json";

            var headerText = $@"{authorizationHeader}
{contentTypeHeader}
";

            var sut = new HeadersMatcher(headerText);

            Assert.Contains(authorizationHeader, sut.Description);
            Assert.Contains(contentTypeHeader, sut.Description);
        }

        private bool Test(HeadersMatcher expected, Action<HttpRequestMessage> actual)
        {
            var request = new HttpRequestMessage(HttpMethod.Get,
                "http://tempuri.org/home");

            actual(request);

            return expected.Matches(request);
        }

    }
}
