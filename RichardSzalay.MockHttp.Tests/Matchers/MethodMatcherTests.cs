using FluentAssertions;
using RichardSzalay.MockHttp.Matchers;

namespace RichardSzalay.MockHttp.Tests.Matchers;

[TestClass]
public class MethodMatcherTests
{
    [TestMethod]
    public void ShouldSucceedOnMatchedMethod()
    {
        bool result = Test(
            expected: HttpMethod.Get,
            actual: HttpMethod.Get
        );

        result.Should().BeTrue();
    }

    [TestMethod]
    public void ShouldFailOnMismatchedMethod()
    {
        bool result = Test(
            expected: HttpMethod.Get,
            actual: HttpMethod.Post
        );
        
        result.Should().BeFalse();
    }

    private static bool Test(HttpMethod expected, HttpMethod actual)
    {
        return new MethodMatcher(expected)
            .Matches(new HttpRequestMessage(actual, "http://tempuri.org/home"));
    }
}