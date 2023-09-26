using FluentAssertions;
using RichardSzalay.MockHttp.Matchers;

namespace RichardSzalay.MockHttp.Tests.Matchers;

[TestClass]
public class ContentMatcherTests
{
    [TestMethod]
    public void SucceedsOnMatchedContent()
    {
        var result = Test(
            expected: "Custom data",
            actual: "Custom data"
        );

        result.Should().BeTrue();
    }

    [TestMethod]
    public void FailsOnUnmatchedContent()
    {
        var result = Test(
            expected: "Custom data",
            actual: "Custom data!"
        );

        result.Should().BeFalse();
    }

    private bool Test(string expected, string actual)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "http://tempuri.org/home");

        request.Content = new StringContent(actual);

        return new ContentMatcher(expected).Matches(request);
    }
}