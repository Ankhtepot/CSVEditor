using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace CSVEditor.Core.Extensions;

public static class PzStringExtensions
{
    extension(string text)
    {
        public char LastChar()
        {
            return string.IsNullOrEmpty(text) ? '\0' : text[^1];
        }

        public bool IsValidUrl()
        {
            const string pattern = @"^(?:http(s)?:\/\/)?[\w.-]+(?:\.[\w\.-]+)+[\w\-\._~:/?#[\]@!\$&'\(\)\*\+,;=.]+$";
            Regex rgx = new(pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
            return rgx.IsMatch(text);
        }
    }

    public static string ToHumanReadableString(this IEnumerable<string> strings, string separator = ",")
    {
        return string.Join(separator, strings);
    }

    extension(string text)
    {
        public bool ContainsAny(char[] chars)
        {
            return chars.Any(text.Contains);
        }

        public string ToSystemPath()
        {
            string path = "";

            if (string.IsNullOrEmpty(text))
            {
                return path;
            }

            path = Regex.Replace(text, "\r", "");
            path = Regex.Replace(path, "/", @"\");

            if (!char.IsLetterOrDigit(path[0]))
            {
                path = path[1..];
            }

            return path;
        }

        public void CreateDirectoryIfNotExists()
        {
            if (!string.IsNullOrEmpty(text) && !Directory.Exists(text))
            {
                Directory.CreateDirectory(text);
            }
        }
    }
}