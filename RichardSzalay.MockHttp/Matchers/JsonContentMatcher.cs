#if NET5_0_OR_GREATER

using System;
using System.IO;
using System.Net.Http;
using System.Text.Json;

namespace RichardSzalay.MockHttp.Matchers
{
    /// <summary>
    /// Matches requests on a predicate-based match of its JSON content
    /// </summary>
    /// <typeparam name="T">The deserialized type that will be used for comparison</typeparam>
    public class JsonContentMatcher<T> : IMockedRequestMatcher
    {
        private readonly JsonSerializerOptions serializerOptions;
        private readonly Func<T, bool> predicate;

        /// <summary>
        /// Constructs a new instance of JsonContentMatcher using a predicate to be used for comparison
        /// </summary>
        /// <param name="predicate">The predicate that will be used to match the deserialized request content</param>
        /// <param name="serializerOptions">Optional. Provide the <see cref="JsonSerializerOptions"/> that will be used to deserialize the request content for comparison.</param>
        public JsonContentMatcher(Func<T, bool> predicate, JsonSerializerOptions serializerOptions = null)
        {
            this.serializerOptions = serializerOptions;
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

            var deserializedContent = Deserialize(message.Content.ReadAsStream());

            return predicate(deserializedContent);
        }

        private T Deserialize(Stream stream)
        {
#if NET5_0
            return JsonSerializer.DeserializeAsync<T>(stream, serializerOptions)
                .GetAwaiter().GetResult();
#else
            return JsonSerializer.Deserialize<T>(stream, serializerOptions);
#endif
        }

    }
}

#endif