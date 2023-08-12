#if NET5_0_OR_GREATER
using RichardSzalay.MockHttp.Matchers;
using System;
using System.Text.Json;

namespace RichardSzalay.MockHttp;

/// <summary>
/// Provides JSON-related extension methods for <see cref="T:MockedRequest"/>
/// </summary>
public static class MockedRequestJsonExtensions
{
    /// <summary>
    /// Requires that the request content contains JSON that matches <paramref name="content"/>
    /// </summary>
    /// <remarks>
    /// The request content must exactly match the serialized JSON of <paramref name="content"/>
    /// </remarks>
    /// <typeparam name="T">The type that represents the JSON request</typeparam>
    /// <param name="source">The source mocked request</param>
    /// <param name="content">The value that, when serialized to JSON, must match the request content</param>
    /// <param name="serializerOptions">Optional. Provide the <see cref="JsonSerializerOptions"/> that will be used to serialize <paramref name="content"/> for comparison.</param>
    /// <returns></returns>
    public static MockedRequest WithJsonContent<T>(this MockedRequest source, T content, JsonSerializerOptions? serializerOptions = null)
    {
        var serializedJson = JsonSerializer.Serialize(content, serializerOptions);

        return source.WithContent(serializedJson);
    }

    /// <summary>
    /// Requires that the request content contains JSON that, when deserialized, matches <paramref name="predicate"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="source">The source mocked request</param>
    /// <param name="predicate">The predicate that will be used to match the deserialized request content</param>
    /// <param name="serializerOptions">Optional. Provide the <see cref="JsonSerializerOptions"/> that will be used to deserialize the request content for comparison.</param>
    /// <returns>The <see cref="T:MockedRequest"/> instance</returns>
    public static MockedRequest WithJsonContent<T>(this MockedRequest source, Func<T, bool> predicate, JsonSerializerOptions? serializerOptions = null)
    {
        return source.With(new JsonContentMatcher<T>(predicate, serializerOptions));
    }
}

#endif