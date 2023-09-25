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
    public class UrlMatcherTests
    {
        [Fact]
        public void Should_match_paths()
        {
            var result = Test(
                expected: "/test",
                actual: "http://tempuri.org/test"
                );

            Assert.True(result);
        }

        [Fact]
        public void Should_not_match_substrings()
        {
            var result = Test(
                expected: "/test",
                actual: "http://tempuri.org/test2"
                );

            Assert.False(result);
        }

        [Fact]
        public void Should_fail_on_unmatched_paths()
        {
            var result = Test(
                expected: "/apple",
                actual: "http://tempuri.org/test"
                );

            Assert.False(result);
        }

        [Fact]
        public void Should_match_full_urls()
        {
            var result = Test(
                expected: "http://tempuri.org/test",
                actual: "http://tempuri.org/test"
                );

            Assert.True(result);
        }

        [Fact]
        public void Should_ignore_query_strings_from_actual()
        {
            var result = Test(
                expected: "http://tempuri.org/test",
                actual: "http://tempuri.org/test?query=value"
                );

            Assert.True(result);
        }

        [Fact]
        public void Should_fail_on_mismatched_urls()
        {
            var result = Test(
                expected: "http://orange.org/orange",
                actual: "http://apple.org/red"
                );

            Assert.False(result);
        }

        [Fact]
        public void Should_match_wildcards_in_paths()
        {
            var result = Test(
                expected: "http://tempuri.org/test1/*/test2",
                actual: "http://tempuri.org/test1/test3/test2"
                );

            Assert.True(result);
        }

        [Fact]
        public void Should_fail_on_mismatched_values_in_wildcards()
        {
            var result = Test(
                expected: "http://tempuri.org/test1/*/test2/*/test3",
                actual: "http://tempuri.org/test1/apple/test3/orange/test2"
                );

            Assert.False(result);
        }

        [Fact]
        public void Should_require_nonblank_vlaues_for_path_wildcards()
        {
            var result = Test(
                expected: "http://tempuri.org/test1/*/test2",
                actual: "http://tempuri.org/test1//atest2"
                );

            Assert.False(result);
        }

        [Fact]
        public void Pathless_absolute_url_matches_pathless_absolute_uri()
        {
            var result = Test(
                expected: "http://tempuri.org",
                actual: "http://tempuri.org"
                );

            Assert.True(result);
        }

        [Fact]
        public void Pathless_absolute_url_matches_root_absolute_uri()
        {
            var result = Test(
                expected: "http://tempuri.org",
                actual: "http://tempuri.org/"
                );

            Assert.True(result);
        }

        [Fact]
        public void Root_absolute_url_matches_pathless_absolute_uri()
        {
            var result = Test(
                expected: "http://tempuri.org/",
                actual: "http://tempuri.org"
                );

            Assert.True(result);
        }

        private bool Test(string expected, string actual)
        {
            return new UrlMatcher(expected)
                .Matches(Request(actual));
        }

        private HttpRequestMessage Request(string url)
        {
            return new HttpRequestMessage(HttpMethod.Get, url);
        }
    }
}
