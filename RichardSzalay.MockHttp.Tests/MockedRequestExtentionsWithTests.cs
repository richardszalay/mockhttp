using System.Net.Http.Headers;
using System.Text;
using FluentAssertions;
using RichardSzalay.MockHttp.Contracts;
using RichardSzalay.MockHttp.Extensions;
using RichardSzalay.MockHttp.Matchers;

namespace RichardSzalay.MockHttp.Tests;

[TestClass]
public class MockedRequestExtensionsWithTests
{
    private MockHttpMessageHandler _mockHandler;

    private HttpRequestMessage _request = new HttpRequestMessage(HttpMethod.Post,
        "http://www.tempuri.org/path?apple=red&pear=green")
    {
        Content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "data1", "value1" },
            { "data2", "value2" }
        })
    };

    [TestInitialize]
    public void SetUp()
    {
        _mockHandler = new MockHttpMessageHandler();
    }

    [TestMethod]
    public void WithQueryStringNameValue()
    {
        TestPass(r => r.WithQueryString("apple", "red"));
        TestFail(r => r.WithQueryString("apple", "green"));
    }

    [TestMethod]
    public void WithQueryStringValuesString()
    {
        TestPass(r => r.WithQueryString("apple=red&pear=green"));
        TestFail(r => r.WithQueryString("apple=green&pear=red"));
    }

    [TestMethod]
    public void WithQueryStringValuesPairs()
    {
        TestPass(r => r.WithQueryString(new Dictionary<string, string>
        {
            { "apple", "red" }
        }));

        TestFail(r => r.WithQueryString(new Dictionary<string, string>
        {
            { "apple", "green" }
        }));
    }

    [TestMethod]
    public void WithFormDataNameValue()
    {
        TestPass(r => r.WithFormData("data1", "value1"));
        TestFail(r => r.WithFormData("data1", "value2"));
    }

    [TestMethod]
    public void WithFormDataValuesString()
    {
        TestPass(r => r.WithFormData("data1=value1&data2=value2"));
        TestFail(r => r.WithFormData("data1=value2&data2=value1"));
    }

    [TestMethod]
    public void WithFormDataValuesPairs()
    {
        TestPass(r => r.WithFormData(new Dictionary<string, string>
        {
            { "data1", "value1" }
        }));

        TestFail(r => r.WithFormData(new Dictionary<string, string>
        {
            { "data1", "value2" }
        }));
    }

    [TestMethod]
    public void WithContent()
    {
        TestPass(r => r.WithContent("data1=value1&data2=value2"));

        TestFail(r => r.WithContent("data1=value2&data2=value1"));
    }

    [TestMethod]
    public void WithPartialContent()
    {
        TestPass(r => r.WithPartialContent("value2"));

        TestFail(r => r.WithPartialContent("value3"));
    }

    [TestMethod]
    public void WithAnyMatchersEnumerable()
    {
        TestPass(r => r.WithAny(new List<IMockedRequestMatcher>
        {
            new QueryStringMatcher("apple=blue"),
            new QueryStringMatcher("apple=red")
        }));

        TestFail(r => r.WithAny(new List<IMockedRequestMatcher>
        {
            new QueryStringMatcher("apple=blue"),
            new QueryStringMatcher("apple=green")
        }));
    }

    [TestMethod]
    public void WithAnyMatchersParams()
    {
        TestPass(r => r.WithAny(
            new QueryStringMatcher("apple=blue"),
            new QueryStringMatcher("apple=red")
        ));

        TestFail(r => r.WithAny(
            new QueryStringMatcher("apple=blue"),
            new QueryStringMatcher("apple=green")
        ));
    }

    [TestMethod]
    public void With()
    {
        TestPass(r =>
            r.With(req => req.Content!.Headers.ContentType!.MediaType == "application/x-www-form-urlencoded"));

        TestFail(r =>
            r.With(req => req.Content!.Headers.ContentType!.MediaType == "text/xml"));
    }

    [TestMethod]
    public void WithHeadersNameValue()
    {
        TestPass(r => r.WithHeaders("Accept", "text/plain"));
        TestFail(r => r.WithHeaders("Accept", "text/xml"));
    }

    [TestMethod]
    public void WithHeadersValuesString()
    {
        TestPass(r => r.WithHeaders(new StringBuilder()
            .AppendLine("Accept: text/plain")
            .AppendLine("Accept-Language: en")
            .ToString()));
        TestFail(r => r.WithHeaders(new StringBuilder()
            .AppendLine("Accept: text/plain")
            .AppendLine("Accept-Language: fr")
            .ToString()));
    }

    [TestMethod]
    public void WithHeadersValuesPairs()
    {
        TestPass(r => r.WithHeaders(new Dictionary<string, string>
        {
            { "Accept", "text/plain" }
        }));

        TestFail(r => r.WithHeaders(new Dictionary<string, string>
        {
            { "Accept", "text/xml" }
        }));
    }

    private void TestPass(Func<MockedRequest, MockedRequest> pass)
    {
        _request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/plain"));
        _request.Headers.AcceptLanguage.Add(new StringWithQualityHeaderValue("en"));

        var result = pass(_mockHandler.Expect("/path"))
            .Matches(_request);

        result.Should().BeTrue();
    }

    private void TestFail(Func<MockedRequest, MockedRequest> fail)
    {
        var result = fail(_mockHandler.Expect("/path"))
            .Matches(_request);

        result.Should().BeFalse();
    }
}