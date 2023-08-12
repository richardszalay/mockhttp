#if NETSTANDARD2_0_OR_GREATER || NET5_0_OR_GREATER
using System;
using System.IO;
using System.Net.Http;
using System.Xml.Serialization;

namespace RichardSzalay.MockHttp.Matchers;

/// <summary>
/// Matches requests on a predicate-based match of its Xml content
/// </summary>
/// <typeparam name="T">The deserialized type that will be used for comparison</typeparam>
public class XmlContentMatcher<T> : IMockedRequestMatcher
{
    private readonly XmlSerializer serializer;
    private readonly Func<T, bool> predicate;

    /// <summary>
    /// Constructs a new instance of XmlContentMatcher using a predicate to be used for comparison
    /// </summary>
    /// <param name="predicate">The predicate that will be used to match the deserialized request content</param>
    /// <param name="serializer">Optional. Provide the <see cref="XmlSerializer"/> that will be used to deserialize the request content for comparison.</param>
    public XmlContentMatcher(Func<T, bool> predicate, XmlSerializer? serializer = null)
    {
        this.serializer = serializer ?? XmlContentMatcher.CreateSerializer(typeof(T));
        this.predicate = predicate;
    }

    /// <summary>
    /// Determines whether the implementation matches a given request
    /// </summary>
    /// <param name="message">The request message being evaluated</param>
    /// <returns>true if the request was matched; false otherwise</returns>
    public bool Matches(HttpRequestMessage message)
    {
        if (message.Content == null)
        {
            return false;
        }

        var stream = message.Content.ReadAsStreamAsync().GetAwaiter().GetResult();
        var deserializedContent = Deserialize(stream);

        if (deserializedContent == null)
        {
            return false;
        }

        return predicate(deserializedContent);
    }

    private T? Deserialize(Stream stream)
    {
        return (T?)serializer.Deserialize(stream);
    }
}

/// <summary>
/// Static class to house an XmlSerializerFactory instance
/// </summary>
public static class XmlContentMatcher
{
    private static XmlSerializerFactory SerializerFactory { get; } = new XmlSerializerFactory();

    /// <summary>
    /// Create a default instance of XmlSerializer for the given type
    /// </summary>
    /// <param name="type">The type to be serialized</param>
    public static XmlSerializer CreateSerializer(Type type)
    {
        return SerializerFactory.CreateSerializer(type);
    }
}
#endif