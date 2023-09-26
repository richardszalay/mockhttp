using FluentAssertions;
using RichardSzalay.MockHttp.Matchers;

namespace RichardSzalay.MockHttp.Tests.Matchers;

[TestClass]
public class PartialContentMatcherTests
{
    [TestMethod]
    [DataRow("Custom data", "Custom data!", true)]
    [DataRow("Custom data!", "Custom data", false)]
    public void Matches_WithData_Returns(string expected, string actual, bool expectedResult)
    {
        var request = new HttpRequestMessage(HttpMethod.Get,
            "http://tempuri.org/home");

        request.Content = new StringContent(actual);

        new PartialContentMatcher(expected).Matches(request)
            .Should().Be(expectedResult);
    }
}