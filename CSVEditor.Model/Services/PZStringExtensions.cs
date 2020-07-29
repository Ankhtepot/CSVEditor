using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace CSVEditor.Model.Services
{
    public static class PZStringExtensions
    {
        public static char LastChar(this string text)
        {
            return string.IsNullOrEmpty(text) ? '\0' : text[text.Length - 1];
        }

        public static bool IsValidURL(this string URL)
        {
            var Pattern = @"^(?:http(s)?:\/\/)?[\w.-]+(?:\.[\w\.-]+)+[\w\-\._~:/?#[\]@!\$&'\(\)\*\+,;=.]+$";
            var Rgx = new Regex(Pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
            return Rgx.IsMatch(URL);
        }

        public static bool ContainsAny(this string text, char[] chars)
        {
            foreach (var character in chars)
            {
                if (text.Contains(character))
                {
                    return true;
                }
            }

            return false;
        }

        public static string ToSystemPath(this string jsonPath)
        {
            var path = "";

            if (string.IsNullOrEmpty(jsonPath))
            {
                return path;
            }

            path = Regex.Replace(jsonPath, "\r", "");
            path = Regex.Replace(path, "/", @"\");

            if (!char.IsLetterOrDigit(path[0]))
            {
                path = path.Substring(1);
            }

            return path;
        }
    }
}
