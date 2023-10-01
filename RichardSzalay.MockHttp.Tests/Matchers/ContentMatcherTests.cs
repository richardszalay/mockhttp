using RichardSzalay.MockHttp.Matchers;
using System;
using System.Net.Http;
using Xunit;

namespace RichardSzalay.MockHttp.Tests.Matchers;

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
    public void ToString_describes_matcher()
    {
        var sut = new ContentMatcher("test");

        var result = sut.ToString();

        Assert.Equal("request body matches test", result);
    }

    private bool Test(string expected, string actual)
    {
        var request = new HttpRequestMessage(HttpMethod.Get,
            "http://tempuri.org/home");

        request.Content = new StringContent(actual);

        return new ContentMatcher(expected).Matches(request);
    }
}
