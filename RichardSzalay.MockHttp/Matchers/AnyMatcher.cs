using RichardSzalay.MockHttp.Formatters;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RichardSzalay.MockHttp.Matchers;

/// <summary>
/// A composite matcher that suceeds if any of it's composed matchers succeed
/// </summary>
public class AnyMatcher : IMockedRequestMatcher, IEnumerable<IMockedRequestMatcher>
{
    readonly IEnumerable<IMockedRequestMatcher> _matchers;

    /// <summary>
    /// Construcuts a new instnace of AnyMatcher
    /// </summary>
    /// <param name="matchers">The list of matchers to evaluate</param>
    public AnyMatcher(IEnumerable<IMockedRequestMatcher> matchers)
        => _matchers = matchers;

    /// <summary>
    /// Determines whether the implementation matches a given request
    /// </summary>
    /// <param name="message">The request message being evaluated</param>
    /// <returns>true if any of the supplied matchers succeed; false otherwise</returns>
    public bool Matches(System.Net.Http.HttpRequestMessage message)
        => _matchers.Any(m => m.Matches(message));

    IEnumerator<IMockedRequestMatcher> IEnumerable<IMockedRequestMatcher>.GetEnumerator()
        => _matchers.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => _matchers.GetEnumerator();

    /// <inheritdoc/>
    public override string ToString()
    {
        var sb = new StringBuilder();

        var first = true;

        foreach (var matcher in _matchers)
        {
            if (first)
            {
                first = false;
                sb.AppendFormat(Resources.AnyMatcherDescriptor, matcher.ToString());
                sb.AppendLine();
            }
            else
            {
                sb.AppendLine($"    OR {matcher.ToString()}");
            }
        }

        return sb.ToString();
        
    }
}
