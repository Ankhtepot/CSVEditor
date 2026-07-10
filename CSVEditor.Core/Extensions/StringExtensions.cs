using System.Collections.Generic;

namespace CSVEditor.Core.Extensions;

public static class StringExtensions
{
    public static string ToHumanReadableString(this IEnumerable<string> strings)
    {
        return string.Join(", ", strings);
    }
}