using System;
using System.Collections.Generic;

namespace RichardSzalay.MockHttp.Matchers;

/// <summary>
/// Provides options on how to match query strings.
/// </summary>
public sealed class QueryStringMatcherOptions
{
    /// <summary>
    /// Constructs a new instance of QueryStringMatcherOptions using the default ordinal comparison on keys and values.
    /// </summary>
    public QueryStringMatcherOptions() : this(StringComparer.Ordinal, StringComparer.Ordinal)
    {
    }

    /// <summary>
    /// Constructs a new instance of QueryStringMatcherOptions using the default ordinal comparison on keys and values.
    /// </summary>
    /// <param name="key">The comparer to use for keys, or null to use the default StringComparer.Ordinal</param>
    /// <param name="value">The comparer to use for values, or null to use the default StringComparer.Ordinal</param>
    public QueryStringMatcherOptions(IEqualityComparer<string>? key = null, IEqualityComparer<string>? value = null)
    {
        KeyComparer = key ?? StringComparer.Ordinal;
        ValueComparer = value ?? StringComparer.Ordinal;
    }

    /// <summary>
    /// The comparer to use for keys
    /// </summary>
    public IEqualityComparer<string> KeyComparer { get; }

    /// <summary>
    /// The comparer to use for values
    /// </summary>
    public IEqualityComparer<string> ValueComparer { get; }
}
