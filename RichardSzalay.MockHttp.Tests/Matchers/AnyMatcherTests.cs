using FluentAssertions;
using RichardSzalay.MockHttp.Contracts;
using RichardSzalay.MockHttp.Matchers;

namespace RichardSzalay.MockHttp.Tests.Matchers;

[TestClass]
public class AnyMatcherTests
{
    [TestMethod]
    public void SucceedsIfFirstMatcherSucceeds()
    {
        var result = Test(true, false);

        result.Should().BeTrue();
    }

    [TestMethod]
    public void SucceedsIfLastMatcherSucceeds()
    {
        var result = Test(false, true);

        result.Should().BeTrue();
    }

    [TestMethod]
    public void FailsIfAllMatchersFail()
    {
        var result = Test(false, false);

        result.Should().BeFalse();
    }

    private static bool Test(params bool[] matcherResults)
    {
        var matchers = matcherResults
            .Select(result => new FakeMatcher(result));

        return new AnyMatcher(matchers)
            .Matches(new HttpRequestMessage());
    }

    private class FakeMatcher : IMockedRequestMatcher
    {
        private bool _result;
        public FakeMatcher(bool result)
        {
            this._result = result;
        }

        public bool Matches(System.Net.Http.HttpRequestMessage message)
        {
            return _result;
        }
    }
}