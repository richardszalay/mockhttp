using System.Net.Http.Headers;
using System.Text;
using FluentAssertions;
using RichardSzalay.MockHttp.Matchers;

namespace RichardSzalay.MockHttp.Tests.Matchers;

[TestClass]
public class HeadersMatcherTests
{
    [TestMethod]
    public void ShouldSucceedOnAllMatched()
    {
        bool result = Test(
            expected: new HeadersMatcher(new Dictionary<string, string>
            {
                { "Authorization", "Basic abcdef" },
                { "Accept", "application/json" },
                { "Content-Type", "text/plain; charset=utf-8" }
            }),
            actual: req =>
            {
                req.Headers.Authorization = new AuthenticationHeaderValue("Basic", "abcdef");
                req.Headers.Accept.Clear();
                req.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                req.Content = new StringContent("test", Encoding.UTF8, "text/plain");
            }
        );

        result.Should().BeTrue();
    }

    [TestMethod]
    public void ShouldParseStringHeaders()
    {
        bool result = Test(
            expected: new HeadersMatcher(new StringBuilder()
                .AppendLine("Accept: application/json")
                .AppendLine("Authorization: Basic abcdef")
                .ToString()),
            actual: req =>
            {
                req.Headers.Authorization = new AuthenticationHeaderValue("Basic", "abcdef");
                req.Headers.Accept.Clear();
                req.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            }
        );

        result.Should().BeTrue();
    }

    private static bool Test(HeadersMatcher expected, Action<HttpRequestMessage> actual)
    {
        var request = new HttpRequestMessage(HttpMethod.Get,
            "http://tempuri.org/home");

        actual(request);

        return expected.Matches(request);
    }
}