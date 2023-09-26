using FluentAssertions;
using RichardSzalay.MockHttp.Matchers;

namespace RichardSzalay.MockHttp.Tests.Matchers;

[TestClass]
public class UrlMatcherTests
{
    [TestMethod]
    [DataRow("/test",  "http://tempuri.org/test", true)]
    [DataRow("/test", "http://tempuri.org/test2", false)]
    [DataRow("/apple", "http://tempuri.org/test", false)]
    [DataRow("http://tempuri.org/test",  "http://tempuri.org/test", true)]
    [DataRow("http://tempuri.org/test", "http://tempuri.org/test?query=value", true)]
    [DataRow("http://orange.org/orange", "http://apple.org/red", false)]
    [DataRow("http://tempuri.org/test1/*/test2", "http://tempuri.org/test1/test3/test2", true)]
    [DataRow("http://tempuri.org/test1/*/test2/*/test3", "http://tempuri.org/test1/apple/test3/orange/test2", false)]
    [DataRow("http://tempuri.org/test1/*/test2", "http://tempuri.org/test1//atest2", false)]
    [DataRow("http://tempuri.org", "http://tempuri.org", true)]
    [DataRow("http://tempuri.org", "http://tempuri.org/", true)]
    [DataRow("http://tempuri.org/", "http://tempuri.org", true)]
    public void Matches_WithData_Returns(string expected, string actual, bool urlMatcherResult)
    {
        var urlMatcher = new UrlMatcher(expected);
        bool result = urlMatcher.Matches(new HttpRequestMessage(HttpMethod.Get, actual));
        
        result.Should().Be(urlMatcherResult);
    }
}