using System;
using System.Collections.Generic;
using System.Net.Http;
using Xunit;
using System.Net.Http.Headers;
using RichardSzalay.MockHttp.Matchers;

namespace RichardSzalay.MockHttp.Tests
{
    /// <summary>
    /// Sanity check tests for MockedRequest extension methods. One pass/fail per overload
    /// </summary>
    public class MockedRequestExtentionsWithTests
    {
        [Fact]
        public void WithQueryString_name_value()
        {
            TestPass(r => r.WithQueryString("apple", "red") );
            TestFail(r => r.WithQueryString("apple", "green"));
        }

        [Fact]
        public void WithQueryString_valuesString()
        {
            TestPass(r => r.WithQueryString("apple=red&pear=green"));
            TestFail(r => r.WithQueryString("apple=green&pear=red"));
        }

        [Fact]
        public void WithQueryString_valuesPairs()
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

        [Fact]
        public void WithFormData_name_value()
        {
            TestPass(r => r.WithFormData("data1", "value1"));
            TestFail(r => r.WithFormData("data1", "value2"));
        }

        [Fact]
        public void WithFormData_valuesString()
        {
            TestPass(r => r.WithFormData("data1=value1&data2=value2"));
            TestFail(r => r.WithFormData("data1=value2&data2=value1"));
        }

        [Fact]
        public void WithFormData_valuesPairs()
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

        [Fact]
        public void WithContent()
        {
            TestPass(r => r.WithContent("data1=value1&data2=value2"));

            TestFail(r => r.WithContent("data1=value2&data2=value1"));
        }

        [Fact]
        public void WithPartialContent()
        {
            TestPass(r => r.WithPartialContent("value2"));

            TestFail(r => r.WithPartialContent("value3"));
        }

        [Fact]
        public void WithAny_matchersEnumerable()
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

        [Fact]
        public void WithAny_matchersParams()
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

        [Fact]
        public void With()
        {
            TestPass(r => 
                r.With(req => req.Content.Headers.ContentType.MediaType == "application/x-www-form-urlencoded"));

            TestFail(r => 
                r.With(req => req.Content.Headers.ContentType.MediaType == "text/xml"));
        }

        [Fact]
        public void WithHeaders_name_value()
        {
            TestPass(r => r.WithHeaders("Accept", "text/plain"));
            TestFail(r => r.WithHeaders("Accept", "text/xml"));
        }

        [Fact]
        public void WithHeaders_valuesString()
        {
            TestPass(r => r.WithHeaders(@"Accept: text/plain
Accept-Language: en"));
            TestFail(r => r.WithHeaders(@"Accept: text/plain
Accept-Language: fr"));
        }

        [Fact]
        public void WithHeaders_valuesPairs()
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

        readonly HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "http://www.tempuri.org/path?apple=red&pear=green")
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "data1", "value1" },
                { "data2", "value2" }
            })
        };

        private void TestPass(Func<MockedRequest, MockedRequest> pass)
        {
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/plain"));
            request.Headers.AcceptLanguage.Add(new StringWithQualityHeaderValue("en"));

            var mockHttp = new MockHttpMessageHandler();

            var result = pass(mockHttp.Expect("/path"))
                .Matches(request);

            Assert.True(result);            
        }

        private void TestFail(Func<MockedRequest, MockedRequest> fail)
        {
            var mockHttp = new MockHttpMessageHandler();

            var result = fail(mockHttp.Expect("/path"))
                .Matches(request);

            Assert.False(result);
        }
    }
}
