using RichardSzalay.MockHttp.Matchers;
using System.Collections.Generic;
using System.Net.Http;
using Xunit;

namespace RichardSzalay.MockHttp.Tests.Matchers;

public class QueryStringMatcherTests
{
    [Fact]
    public void Should_match_in_order()
    {
        bool result = Test(
            expected: "key1=value1&key2=value2",
            actual: "key1=value1&key2=value2"
            );

        Assert.True(result);
    }

    [Fact]
    public void Should_match_out_of_order()
    {
        bool result = Test(
            expected: "key2=value2&key1=value1",
            actual: "key1=value1&key2=value2"
            );

        Assert.True(result);
    }

    [Fact]
    public void Should_match_multiple_values()
    {
        bool result = Test(
            expected: "key1=value1&key1=value2",
            actual: "key1=value2&key1=value1"
            );

        Assert.True(result);
    }

    [Fact]
    public void Should_support_matching_empty_values()
    {
        bool result = Test(
            expected: "key2=value2&key1",
            actual: "key1&key2=value2"
            );

        Assert.True(result);
    }

    [Fact]
    public void Should_fail_for_incorrect_values()
    {
        bool result = Test(
            expected: "key1=value1&key2=value3",
            actual: "key1=value1&key2=value2"
            );

        Assert.False(result);
    }

    [Fact]
    public void Should_fail_for_missing_keys()
    {
        bool result = Test(
            expected: "key2=value2&key1=value1",
            actual: "key1=value1&key3=value3"
            );

        Assert.False(result);
    }

    [Fact]
    public void Should_not_fail_for_additional_keys_when_exact_is_false()
    {
        bool result = Test(
            expected: "key1=value1&key2=value2",
            actual: "key1=value1&key2=value2&key3=value3"
            );

        Assert.True(result);
    }

    [Fact]
    public void Should_fail_for_additional_keys_when_exact_is_true()
    {
        bool result = Test(
            expected: "key1=value1&key2=value2",
            actual: "key1=value1&key2=value2&key3=value3",
            exact: true
            );

        Assert.False(result);
    }

    [Fact]
    public void Should_support_matching_dictionary_data_with_url_encoded_values()
    {
        var data = new Dictionary<string, string>();
        data.Add("key", "Value with spaces");

        var qs = "key=Value+with%20spaces";

        var sut = new QueryStringMatcher(data);

        var actualMatch = sut.Matches(new HttpRequestMessage(HttpMethod.Get, "http://tempuri.org/home?" + qs));

        Assert.True(actualMatch, "QueryStringMatcher.Matches() should match dictionary data with URL encoded query string values.");
    }

    private bool Test(string expected, string actual, bool exact = false)
    {
        var sut = new QueryStringMatcher(expected, exact);

        return sut.Matches(new HttpRequestMessage(HttpMethod.Get,
            "http://tempuri.org/home?" + actual));
    }

    [Fact]
    public void ToString_describes_partial_matcher()
    {
        var sut = new QueryStringMatcher("key=Value+with%20spaces&b=2", exact: false);

        var result = sut.ToString();

        Assert.Equal("query string matches key=Value%20with%20spaces&b=2", result);
    }

    [Fact]
    public void ToString_describes_exact_matcher()
    {
        var sut = new QueryStringMatcher("key=Value+with%20spaces&b=2", exact: true);

        var result = sut.ToString();

        Assert.Equal("query string exacly matches (no additional keys allowed) key=Value%20with%20spaces&b=2", result);
    }
}
