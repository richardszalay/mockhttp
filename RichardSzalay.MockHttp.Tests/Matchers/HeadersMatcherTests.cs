using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
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
                        { "Accept", "application/json" }
                    }),
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

        private bool Test(HeadersMatcher expected, Action<HttpRequestMessage> actual)
        {
            var request = new HttpRequestMessage(HttpMethod.Get,
                "http://tempuri.org/home");

            actual(request);

            return expected.Matches(request);
        }

    }
}
