using System;
using System.Diagnostics.CodeAnalysis;

namespace RichardSzalay.MockHttp;

internal static class UriUtil
{
    public static bool IsWellFormedUriString(string url, UriKind kind)
    {
        return TryParse(url, kind, out _);

    }

    // MonoAndroid parses relative URI's and adds a "file://" protocol, which causes the matcher to fail
    public static bool TryParse(string url, UriKind kind, [NotNullWhen(true)] out Uri? output)
    {
        if (!Uri.TryCreate(url, kind, out output))
        {
            return false;
        }

        bool isAndroidFalsePositive = output.Scheme == "file" && !url.StartsWith("file://", StringComparison.Ordinal);

        return !isAndroidFalsePositive;
    }
}
