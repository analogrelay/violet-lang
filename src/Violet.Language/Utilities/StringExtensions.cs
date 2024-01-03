namespace System;

public static class StringExtensions
{
    public static string Unescape(this string str)
    {
        return str
            .Replace("\n", "\\n")
            .Replace("\r", "\\r")
            .Replace("\t", "\\t");
    }
}
