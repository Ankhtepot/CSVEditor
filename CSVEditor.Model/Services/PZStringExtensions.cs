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

        public static bool ContainsAny(this string text, List<char> chars)
        {

        }
    }
}
