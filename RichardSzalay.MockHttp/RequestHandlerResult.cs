using System.Collections.Generic;
using System.Net.Http;

namespace RichardSzalay.MockHttp
{
    internal class RequestHandlerResult
    {
        public RequestHandlerResult(HttpRequestMessage request)
        {
            this.Request = request;
        }

        public HttpRequestMessage Request { get; }
        public IMockedRequest? Handler { get; set; }

        public MockedRequestResult? RequestExpectationResult { get; set; }
        public List<IMockedRequest> UnevaluatedRequestExpectations { get; } = new();

        public List<MockedRequestResult> BackendDefinitionResults { get; } = new();
        public List<IMockedRequest> UnevaluatedBackendDefinitions { get; } = new();
    }

    internal class MockedRequestResult
    {
        private MockedRequestResult()
        {
        }

        public IMockedRequest? Handler { get; set; } = default!;

        public Dictionary<IMockedRequestMatcher, bool> MatcherResults { get; private set; } = new();
        public bool Success { get; internal set; }

        internal static MockedRequestResult FromMatcherResults(IMockedRequest handler, Dictionary<IMockedRequestMatcher, bool> matcherResults, bool success)
        {
            return new()
            {
                Handler = handler,
                MatcherResults = matcherResults,
                Success = success
            };
        }

        internal static MockedRequestResult FromResult(IMockedRequest handler, bool success)
        {
            return new()
            {
                Handler = handler,
                Success = success
            };
        }
    }

    internal class MockedRequestMatcherResult
    {
    }
}