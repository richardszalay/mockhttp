using RichardSzalay.MockHttp.Matchers;
using System;
using System.Collections.Generic;
using Xunit;

namespace RichardSzalay.MockHttp.Tests.Matchers;

public class QueryStringMatcherOptionsTests
{
    [Fact]
    public void Should_default_to_ordinal_key_comparison()
    {
        // act
        var sut = new QueryStringMatcherOptions();

        // assert
        Assert.Equal(StringComparer.Ordinal, sut.KeyComparer);
    }

    [Fact]
    public void Should_default_to_ordinal_value_comparison()
    {
        // act
        var sut = new QueryStringMatcherOptions();

        // assert
        Assert.Equal(StringComparer.Ordinal, sut.ValueComparer);
    }

    [Theory]
    [MemberData(nameof(ConstructorTheoryData))]
    public void Should_initialize_comparers_correctly(IEqualityComparer<string>? key, IEqualityComparer<string>? value, IEqualityComparer<string> expectedKey, IEqualityComparer<string> expectedValue)
    {
        // act
        var sut = new QueryStringMatcherOptions(key, value);

        // assert
        Assert.Equal(expectedKey, sut.KeyComparer);
        Assert.Equal(expectedValue, sut.ValueComparer);
    }

    public static IEnumerable<object?[]> ConstructorTheoryData
    {
        get
        {
            // nulls default to Ordinal
            yield return new object?[] { null, null, StringComparer.Ordinal, StringComparer.Ordinal };
            // set value comparer, should be correct
            yield return new object?[] { null, StringComparer.OrdinalIgnoreCase, StringComparer.Ordinal, StringComparer.OrdinalIgnoreCase };
            // set value comparer, should be correct
            yield return new object?[] { StringComparer.OrdinalIgnoreCase, null, StringComparer.OrdinalIgnoreCase, StringComparer.Ordinal };
            // set both comparers, should be correct
            yield return new object?[] { StringComparer.OrdinalIgnoreCase, StringComparer.InvariantCultureIgnoreCase, StringComparer.OrdinalIgnoreCase, StringComparer.InvariantCultureIgnoreCase };
        }
    }
}
