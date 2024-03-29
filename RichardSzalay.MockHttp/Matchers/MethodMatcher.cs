﻿using RichardSzalay.MockHttp.Formatters;
using System.Net.Http;

namespace RichardSzalay.MockHttp.Matchers;

/// <summary>
/// Matches requests based on their HTTP method
/// </summary>
public class MethodMatcher : IMockedRequestMatcher
{
    readonly HttpMethod method;

    /// <summary>
    /// Constructs a new instance of MethodMatcher
    /// </summary>
    /// <param name="method">The method to match against</param>
    public MethodMatcher(HttpMethod method)
    {
        this.method = method;
    }

    /// <summary>
    /// Determines whether the implementation matches a given request
    /// </summary>
    /// <param name="message">The request message being evaluated</param>
    /// <returns>true if the request was matched; false otherwise</returns>
    public bool Matches(HttpRequestMessage message)
    {
        return message.Method == method;
    }

    /// <inheritdoc/>
    public override string ToString()
        => string.Format(Resources.MethodMatcherDescriptor, method.ToString());
}
