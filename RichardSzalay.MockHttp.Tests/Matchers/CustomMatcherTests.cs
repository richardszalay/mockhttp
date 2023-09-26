using FluentAssertions;
using RichardSzalay.MockHttp.Matchers;

namespace RichardSzalay.MockHttp.Tests.Matchers;

[TestClass]
public class CustomMatcherTests
{
    [TestMethod]
    public void ShouldSucceedWhenHandlerSucceeds()
    {
        var result = Test(r => true);

        result.Should().BeTrue();
    }

    [TestMethod]
    public void ShouldFailWhenHandlerFails()
    {
        var result = Test(r => false);

        result.Should().BeFalse();
    }

    private bool Test(Func<HttpRequestMessage, bool> handler) => new CustomMatcher(handler)
        .Matches(new HttpRequestMessage(HttpMethod.Get, "http://tempuri.org/home"));
}