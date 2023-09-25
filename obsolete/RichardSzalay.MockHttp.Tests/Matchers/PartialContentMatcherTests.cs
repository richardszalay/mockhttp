﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using RichardSzalay.MockHttp.Matchers;
using Xunit;

namespace RichardSzalay.MockHttp.Tests.Matchers
{
    public class PartialContentMatcherTests
    {
        [Fact]
        public void Succeeds_on_partially_matched_content()
        {
            var result = Test(
                expected: "Custom data",
                actual: "Custom data!"
            );

            Assert.True(result);
        }

        [Fact]
        public void Fails_on_unmatched_content()
        {
            var result = Test(
                expected: "Custom data!",
                actual: "Custom data"
            );

            Assert.False(result);
        }

        private bool Test(string expected, string actual)
        {
            var request = new HttpRequestMessage(HttpMethod.Get,
                "http://tempuri.org/home");

            request.Content = new StringContent(actual);

            return new PartialContentMatcher(expected).Matches(request);
        }
    }
}
