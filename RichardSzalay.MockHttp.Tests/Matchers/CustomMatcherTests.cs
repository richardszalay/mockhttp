﻿using RichardSzalay.MockHttp.Matchers;
using System;
using System.Net.Http;
using Xunit;

namespace RichardSzalay.MockHttp.Tests.Matchers;

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

    [Fact]
    public void ToString_describes_matcher()
    {
        var sut = new CustomMatcher(_ => true);

        var result = sut.ToString();

        Assert.Equal("request matches a custom predicate", result);
    }

    private bool Test(Func<HttpRequestMessage, bool> handler)
    {
        var sut = new CustomMatcher(handler);

        return sut.Matches(new HttpRequestMessage(HttpMethod.Get,
            "http://tempuri.org/home"));
    }
}
