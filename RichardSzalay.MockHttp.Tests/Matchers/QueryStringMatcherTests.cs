using FluentAssertions;
using RichardSzalay.MockHttp.Matchers;

namespace RichardSzalay.MockHttp.Tests.Matchers;

[TestClass]
public class QueryStringMatcherTests
{
    [TestMethod]
    [DataRow("key1=value1&key2=value2", "key1=value1&key2=value2", false, true)]
    [DataRow("key2=value2&key1=value1", "key1=value1&key2=value2", false, true)]
    [DataRow("key1=value1&key1=value2", "key1=value2&key1=value1", false, true)]
    [DataRow("key2=value2&key1", "key1&key2=value2", false, true)]
    [DataRow("key1=value1&key2=value3", "key1=value1&key2=value2", false, false)]
    [DataRow("key2=value2&key1=value1", "key1=value1&key3=value3", false, false)]
    [DataRow("key1=value1&key2=value2", "key1=value1&key2=value2&key3=value3", false, true)]
    [DataRow("key1=value1&key2=value2", "key1=value1&key2=value2&key3=value3", true, false)]
    public void Matches_WithData_Returns(string expected, string actual, bool exact, bool expectedResult)
    {
        var sut = new QueryStringMatcher(expected, exact);

        var result = sut.Matches(new HttpRequestMessage(HttpMethod.Get, "http://tempuri.org/home?" + actual));
        
        result.Should().Be(expectedResult);
    }

    /// <summary>
    /// QueryStringMatcher.Matches() should match dictionary data with URL encoded query string values.
    /// </summary>
    [TestMethod]
    public void ShouldSupportMatchingDictionaryDataWithUrlEncodedValues()
    {
        var data = new Dictionary<string, string>
        {
            { "key", "Value with spaces" }
        };

        var requestUrl = "http://tempuri.org/home?key=Value+with%20spaces";

        new QueryStringMatcher(data)
            .Matches(new HttpRequestMessage(HttpMethod.Get, requestUrl))
            .Should().BeTrue();
    }
}