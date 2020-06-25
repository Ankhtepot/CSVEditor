using System;
using System.Collections.Generic;
using System.Text;

namespace CSVEditor.Model.Services
{
    public static class PZStringExtensions
    {
        public static char LastChar(this string text)
        {
            return string.IsNullOrEmpty(text) ? '\0' : text[text.Length - 1];
        }
    }
}
