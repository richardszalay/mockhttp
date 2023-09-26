using System.Net.Http.Headers;
using FluentAssertions;
using RichardSzalay.MockHttp.Matchers;
using RichardSzalay.MockHttp.Tests.Infrastructure;

namespace RichardSzalay.MockHttp.Tests.Matchers;

[TestClass]
public class FormDataMatcherTests
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
        FormUrlEncodedContent content = new(
            HttpHelpers.ParseQueryString(actual)
        );

        var result = new FormDataMatcher(expected, exact)
            .Matches(new HttpRequestMessage(HttpMethod.Get, "http://tempuri.org/home")
            {
                Content = content
            });

        result.Should().Be(expectedResult);
    }

    /// <summary>
    /// FormDataMatcher.Matches() should match dictionary data with URL encoded query string values.
    /// </summary>
    [TestMethod]
    public void ShouldSupportMatchingDictionaryDataWithUrlEncodedValues1()
    {
        var data = new Dictionary<string, string>
        {
            { "key", "Value with spaces" }
        };

        var content = new FormUrlEncodedContent(data);
        var actualMatch = new FormDataMatcher(data)
            .Matches(new HttpRequestMessage(HttpMethod.Get, "http://tempuri.org/home")
        {
            Content = content
        });

        actualMatch.Should().BeTrue();
    }

    /// <summary>
    /// FormDataMatcher.Matches() should match dictionary data with URL encoded query string values.
    /// </summary>
    [TestMethod]
    public void ShouldSupportMatchingDictionaryDataWithUrlEncodedValues2()
    {
        var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "key", "Value with spaces" }
        });

        var actualMatch = new FormDataMatcher("key=Value+with%20spaces")
            .Matches(new HttpRequestMessage(HttpMethod.Get, "http://tempuri.org/home")
        {
            Content = content
        });

        actualMatch.Should().BeTrue();
    }

    [TestMethod]
    public void Should_fail_for_non_form_data()
    {
        var content = new FormUrlEncodedContent(HttpHelpers.ParseQueryString("key=value"));
        content.Headers.ContentType = new MediaTypeHeaderValue("text/plain");

        var result = Test(
            expected: "key=value",
            actual: content
        );

        result.Should().BeFalse();
    }

    [TestMethod]
    public void Supports_multipart_formdata_content()
    {
        var content = new MultipartFormDataContent
        {
            new FormUrlEncodedContent(HttpHelpers.ParseQueryString("key=value"))
        };

        var result = Test(
            expected: "key=value",
            actual: content
        );

        result.Should().BeTrue();
    }

    [TestMethod]
    public void Matches_form_data_across_multipart_entries()
    {
        var content = new MultipartFormDataContent
        {
            new FormUrlEncodedContent(HttpHelpers.ParseQueryString("key1=value1")),
            new FormUrlEncodedContent(HttpHelpers.ParseQueryString("key2=value2"))
        };

        var result = Test(
            expected: "key1=value1&key2=value2",
            actual: content
        );

        result.Should().BeTrue();
    }

    [TestMethod]
    public void Does_not_match_form_data_on_non_form_data_multipart_entries()
    {
        var content = new MultipartFormDataContent
        {
            new FormUrlEncodedContent(HttpHelpers.ParseQueryString("key1=value1")),
            new FormUrlEncodedContent(HttpHelpers.ParseQueryString("key2=value2"))
        };

        content.First().Headers.ContentType = new MediaTypeHeaderValue("text/plain");

        var result = Test(
            expected: "key1=value1&key2=value2",
            actual: content
        );

        result.Should().BeFalse();
    }

    private static bool Test(string expected, HttpContent actual) => new FormDataMatcher(expected)
        .Matches(new HttpRequestMessage(HttpMethod.Get, "http://tempuri.org/home")
        {
            Content = actual
        });
}