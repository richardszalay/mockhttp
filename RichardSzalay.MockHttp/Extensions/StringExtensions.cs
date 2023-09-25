namespace RichardSzalay.MockHttp.Extensions;

internal static class StringExtensions
{
    public static string[] Split(this string input, char c, int count)
    {
        int index = input.IndexOf(c);

        return index == -1
            ? new[] { input }
            : new[] { input.Substring(0, index), input.Substring(index + 1) };
    }
}