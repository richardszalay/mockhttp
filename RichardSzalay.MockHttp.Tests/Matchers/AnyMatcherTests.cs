using RichardSzalay.MockHttp.Matchers;
using System.Linq;
using Xunit;

namespace RichardSzalay.MockHttp.Tests.Matchers;

public class AnyMatcherTests
{
    [Fact]
    public void Succeeds_if_first_matcher_succeeds()
    {
        var result = Test(true, false);

        Assert.True(result);
    }

    [Fact]
    public void Succeeds_if_last_matcher_succeeds()
    {
        var result = Test(false, true);

        Assert.True(result);
    }

    [Fact]
    public void Fails_if_all_matchers_fail()
    {
        var result = Test(false, false);

        Assert.False(result);
    }

    public bool Test(params bool[] matcherResults)
    {
        var matchers = matcherResults
            .Select(result => new FakeMatcher(result));

        return new AnyMatcher(matchers).Matches(new System.Net.Http.HttpRequestMessage());
    }

    private class FakeMatcher : IMockedRequestMatcher
    {
        private bool _result;
        public FakeMatcher(bool result)
        {
            _result = result;
        }

        public bool Matches(System.Net.Http.HttpRequestMessage message)
        {
            return _result;
        }
    }
}
