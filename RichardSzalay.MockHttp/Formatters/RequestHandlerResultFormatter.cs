using RichardSzalay.MockHttp.Matchers;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;

namespace RichardSzalay.MockHttp.Formatters
{
    internal class RequestHandlerResultFormatter
    {
        public static string FormatRequestMessage(HttpRequestMessage request) =>
            $"{request.Method} {request.RequestUri?.AbsoluteUri}";

        public static string Format(RequestHandlerResult result)
        {
            var sb = new StringBuilder();

            if (result.Handler != null)
            {
                sb.AppendLine(string.Format(Resources.MatchSuccessHeader, FormatRequestMessage(result.Request)));
            }
            else
            {
                sb.AppendLine(string.Format(Resources.MatchFailureHeader, FormatRequestMessage(result.Request)));
            }

            if (result.RequestExpectationResult != null)
            {
                if (result.RequestExpectationResult.Success)
                {
                    sb.AppendLine(Resources.RequestExpectationMatchSuccessHeader);
                }
                else
                {
                    sb.AppendLine(Resources.RequestExpectationMatchFailureHeader);
                }

                sb.AppendLine();

                MockedRequestFormatter.FormatWithResult(sb, result.RequestExpectationResult);

                if (result.UnevaluatedRequestExpectations.Count > 0)
                {
                    sb.AppendLine(string.Format(Resources.SkippedRequestExpectationsHeader, result.UnevaluatedRequestExpectations.Count));
                    sb.AppendLine();
                }

                if (result.BackendDefinitionResults.Count > 0)
                {
                    sb.AppendLine(Resources.BackendDefinitionFallbackHeader);
                }
                else if (result.UnevaluatedBackendDefinitions.Count > 0)
                {
                    sb.AppendLine(string.Format(Resources.NoBackendDefinitionFallbackHeader, result.UnevaluatedBackendDefinitions.Count));
                }
            }

            var failedBackendDefinitionsResults = result.BackendDefinitionResults.TakeWhile(r => !r.Success).ToList();

            if (failedBackendDefinitionsResults.Count > 0)
            {
                sb.AppendLine(string.Format(Resources.BackendDefinitionsMatchFailedHeader, failedBackendDefinitionsResults.Count));
                sb.AppendLine();

                foreach (var failedBackendDefinitionResult in failedBackendDefinitionsResults)
                {
                    MockedRequestFormatter.FormatWithResult(sb, failedBackendDefinitionResult);
                    sb.AppendLine();
                }
            }

            var matchedBackendDefinitionResult = result.BackendDefinitionResults
                .FirstOrDefault(r => object.ReferenceEquals(r.Handler, result.Handler));

            if (matchedBackendDefinitionResult != null)
            {
                sb.AppendLine(Resources.BackendDefinitionMatchSuccessHeader);
                sb.AppendLine();
                MockedRequestFormatter.FormatWithResult(sb, matchedBackendDefinitionResult);
            }

            return sb.ToString();
        }
    }

    internal class MockedRequestFormatter
    {
        public static void FormatWithResult(StringBuilder sb, MockedRequestResult result)
        {
            string GetMatcherStatus(IMockedRequestMatcher matcher)
            {
                if (result.MatcherResults.TryGetValue(matcher, out var matcherResult))
                {
                    return matcherResult ? Resources.MatcherStatusSuccessLabel : Resources.MatcherStatusFailedLabel;
                }

                return Resources.MatcherStatusSkippedLabel;
            }

            if (result.Handler is not IEnumerable<IMockedRequestMatcher> matchers)
            {
                sb.AppendLine(result.Handler.ToString());
                return;
            }

            void FormatAllMatchers(IEnumerable<IMockedRequestMatcher> matchers, string joiner, int indent)
            {
                bool first = true;
                foreach (var matcher in matchers)
                {
                    if (first)
                    {
                        first = false;
                    }
                    else
                    {
                        sb.Append(' ', indent);
                        sb.Append(joiner);
                    }

                    if (matcher is AnyMatcher anyMatcher)
                    {
                        sb.Append($"[{GetMatcherStatus(matcher)}] {matcher}");
                        FormatAllMatchers(anyMatcher, "OR ", indent + 4);
                    }
                    else
                    {
                        sb.AppendLine($"[{GetMatcherStatus(matcher)}] {matcher}");
                    }
                }
            }

            FormatAllMatchers(matchers, "AND ", 4);
        }
    }
}
