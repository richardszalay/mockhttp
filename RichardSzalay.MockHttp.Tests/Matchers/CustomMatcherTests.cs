﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using RichardSzalay.MockHttp.Matchers;
using Xunit;

namespace RichardSzalay.MockHttp.Tests.Matchers
{
    public class CustomMatcherTests
    {
        [Fact]
        public void Should_succeed_when_handler_succeeds()
        {
            bool result = Test(r => true);

            Assert.True(result);
        }

        [Fact]
        public void Should_fail_when_handler_fails()
        {
            bool result = Test(r => false);

            Assert.False(result);
        }

        private bool Test(Func<HttpRequestMessage, bool> handler, [CallerArgumentExpression("handler")] string handlerText = "")
        {
            var sut = new CustomMatcher(handler, handlerText);

            return sut.Matches(new HttpRequestMessage(HttpMethod.Get, 
                "http://tempuri.org/home"));
        }
    }
}
