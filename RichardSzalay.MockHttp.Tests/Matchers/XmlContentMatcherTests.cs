using RichardSzalay.MockHttp.Matchers;
using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Xml.Serialization;
using Xunit;

namespace RichardSzalay.MockHttp.Tests.Matchers;

public class XmlContentMatcherTests
{
    private static XmlSerializer xmlSerializer = new XmlSerializer(typeof(XmlContent));

    [Fact]
    public void Should_succeed_when_predicate_returns_true()
    {
        var result = Test(
            expected: c => c.Value == true,
            actual: new XmlContent { Value = true }
            );

        Assert.True(result);
    }

    [Fact]
    public void Should_fail_when_predicate_returns_false()
    {
        var result = Test(
            expected: c => c.Value == false,
            actual: new XmlContent { Value = true }
            );

        Assert.False(result);
    }

    private bool Test(Func<XmlContent, bool> expected, XmlContent actual)
    {
        var sut = new XmlContentMatcher<XmlContent>(expected);

        var ms = new MemoryStream();
        var sw = new StreamWriter(ms);
        xmlSerializer.Serialize(sw, actual);

        StringContent content = new StringContent(Encoding.UTF8.GetString(ms.ToArray()));

        return sut.Matches(new HttpRequestMessage(HttpMethod.Get,
            "http://tempuri.org/home")
        { Content = content });
    }

    public class XmlContent
    {
        public bool Value { get; set; }
    }
}
