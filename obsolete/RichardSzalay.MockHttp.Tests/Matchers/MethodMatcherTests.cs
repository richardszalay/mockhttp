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
    public class MethodMatcherTests
    {
        [Fact]
        public void Should_succeed_on_matched_method()
        {
            bool result = Test(
                expected: HttpMethod.Get,
                actual: HttpMethod.Get
                );

            Assert.True(result);
        }

        [Fact]
        public void Should_fail_on_mismatched_method()
        {
            bool result = Test(
                expected: HttpMethod.Get,
                actual: HttpMethod.Post
                );

            Assert.False(result);
        }

        private bool Test(HttpMethod expected, HttpMethod actual)
        {
            var sut = new MethodMatcher(expected);

            return sut.Matches(new HttpRequestMessage(actual, 
                "http://tempuri.org/home"));
        }
    }
}
