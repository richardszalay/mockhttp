using RichardSzalay.MockHttp.Matchers;

namespace RichardSzalay.MockHttp.Extensions;

/// <summary>
/// Provides extension methods for <see cref="T:MockHttpMessageHandler"/>
/// </summary>
public static class MockHttpMessageHandlerExtensions
{
    /// <summary>
    /// Adds a backend definition 
    /// </summary>
    /// <param name="handler">The source handler</param>
    /// <param name="method">The HTTP method to match</param>
    /// <param name="url">The URL (absolute or relative, may contain * wildcards) to match</param>
    /// <returns>The <see cref="T:MockedRequest"/> instance</returns>
    public static MockedRequest When(this MockHttpMessageHandler handler, HttpMethod method, string url)
    {
        MockedRequest message = new(url);
        message.With(new MethodMatcher(method));

        handler.AddBackendDefinition(message);

        return message;
    }

    /// <summary>
    /// Adds a backend definition 
    /// </summary>
    /// <param name="handler">The source handler</param>
    /// <param name="url">The URL (absolute or relative, may contain * wildcards) to match</param>
    /// <returns>The <see cref="T:MockedRequest"/> instance</returns>
    public static MockedRequest When(this MockHttpMessageHandler handler, string url)
    {
        MockedRequest message = new(url);

        handler.AddBackendDefinition(message);

        return message;
    }

    /// <summary>
    /// Adds a request expectation
    /// </summary>
    /// <param name="handler">The source handler</param>
    /// <param name="method">The HTTP method to match</param>
    /// <param name="url">The URL (absolute or relative, may contain * wildcards) to match</param>
    /// <returns>The <see cref="T:MockedRequest"/> instance</returns>
    public static MockedRequest Expect(this MockHttpMessageHandler handler, HttpMethod method, string url)
    {
        MockedRequest message = new(url);
        message.With(new MethodMatcher(method));

        handler.AddRequestExpectation(message);

        return message;
    }

    /// <summary>
    /// Adds a request expectation
    /// </summary>
    /// <param name="handler">The source handler</param>
    /// <param name="url">The URL (absolute or relative, may contain * wildcards) to match</param>
    /// <returns>The <see cref="T:MockedRequest"/> instance</returns>
    public static MockedRequest Expect(this MockHttpMessageHandler handler, string url)
    {
        MockedRequest message = new(url);

        handler.AddRequestExpectation(message);

        return message;
    }
}