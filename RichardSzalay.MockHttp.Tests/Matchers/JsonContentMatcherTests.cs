using RichardSzalay.MockHttp.Matchers;
using System;
using System.Net.Http;
using System.Text.Json;
using Xunit;

namespace RichardSzalay.MockHttp.Tests.Matchers;

public class JsonContentMatcherTests
{
    [Fact]
    public void Should_succeed_when_predicate_returns_true()
    {
        var result = Test(
            expected: c => c.Value == true,
            actual: new JsonContent(true)
            );

        Assert.True(result);
    }

    [Fact]
    public void Should_fail_when_predicate_returns_false()
    {
        var result = Test(
            expected: c => c.Value == false,
            actual: new JsonContent(true)
            );

        Assert.False(result);
    }

    private bool Test(Func<JsonContent, bool> expected, JsonContent actual)
    {
        var options = new JsonSerializerOptions();

        var sut = new JsonContentMatcher<JsonContent>(expected, options);

        StringContent content = new StringContent(
            JsonSerializer.Serialize(actual, options)
            );

        return sut.Matches(new HttpRequestMessage(HttpMethod.Get,
            "http://tempuri.org/home")
        { Content = content });
    }

    public record JsonContent(bool Value);
}
