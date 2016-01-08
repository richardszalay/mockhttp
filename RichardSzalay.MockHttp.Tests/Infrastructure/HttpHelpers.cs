using System;
using System.Collections.Generic;
using System.Linq;

namespace RichardSzalay.MockHttp.Tests.Infrastructure
{
    public static class HttpHelpers
    {
        internal static IEnumerable<KeyValuePair<string, string>> ParseQueryString(string input)
        {
            return input.TrimStart('?').Split('&')
                .Select(pair => pair.Split(new [] { '=' }, 2))
                .Select(pair => new KeyValuePair<string, string>(
                    Uri.UnescapeDataString(pair[0]),
                    pair.Length == 2 ? Uri.UnescapeDataString(pair[1]) : null
                    ))
                .ToList();
        }
    }
}
