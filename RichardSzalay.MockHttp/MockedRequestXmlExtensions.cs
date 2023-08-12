#if NETSTANDARD2_0_OR_GREATER || NET5_0_OR_GREATER
using RichardSzalay.MockHttp.Matchers;
using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace RichardSzalay.MockHttp;

/// <summary>
/// Provides XML-related extension methods for <see cref="T:MockedRequest"/>
/// </summary>
public static class MockedRequestXmlExtensions
{
    /// <summary>
    /// Requires that the request content contains Xml that matches <paramref name="content"/>
    /// </summary>
    /// <remarks>
    /// The request content must exactly match the serialized Xml of <paramref name="content"/>
    /// </remarks>
    /// <typeparam name="T">The type that represents the Xml request</typeparam>
    /// <param name="source">The source mocked request</param>
    /// <param name="content">The value that, when serialized to Xml, must match the request content</param>
    /// <param name="serializer">Optional. Provide the <see cref="XmlSerializer"/> that will be used to serialize <paramref name="content"/> for comparison.</param>
    /// <param name="settings">Optional. Provide the <see cref="XmlWriterSettings"/> that will be used to serialize <paramref name="content"/> for comparison.</param>
    /// <returns></returns>
    public static MockedRequest WithXmlContent<T>(this MockedRequest source, T content, XmlSerializer? serializer = null, XmlWriterSettings? settings = null)
    {
        serializer = serializer ?? XmlContentMatcher.CreateSerializer(typeof(T));

        var ms = new MemoryStream();
        var writer = new StreamWriter(ms);
        var xmlWriter = XmlWriter.Create(writer, settings);
        serializer.Serialize(xmlWriter, content);

        return source.WithContent(Encoding.UTF8.GetString(ms.ToArray()));
    }

    /// <summary>
    /// Requires that the request content contains Xml that, when deserialized, matches <paramref name="predicate"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="source">The source mocked request</param>
    /// <param name="predicate">The predicate that will be used to match the deserialized request content</param>
    /// <param name="serializer">Optional. Provide the <see cref="XmlSerializer"/> that will be used to deserialize the request content for comparison.</param>
    /// <returns>The <see cref="T:MockedRequest"/> instance</returns>
    public static MockedRequest WithXmlContent<T>(this MockedRequest source, Func<T, bool> predicate, XmlSerializer? serializer = null)
    {
        return source.With(new XmlContentMatcher<T>(predicate, serializer));
    }
}

#endif