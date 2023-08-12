using System;

namespace RichardSzalay.MockHttp
{
    internal static class UriUtil
    {
        public static bool IsWellFormedUriString(string url, UriKind kind)
        {
            Uri output;
            return TryParse(url, kind, out output);

        }

        // MonoAndroid parses relative URI's and adds a "file://" protocol, which causes the matcher to fail
        public static bool TryParse(string url, UriKind kind, out Uri output)
        {
            bool systemResult = Uri.TryCreate(url, kind, out output);

            bool isAndroidFalsePositive = systemResult && output.Scheme == "file" && !url.StartsWith("file://", StringComparison.Ordinal);

            return systemResult && !isAndroidFalsePositive;
        }
    }
}
