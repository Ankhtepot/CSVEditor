using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CSVEditor.Services
{
    public class FileProcessingServices
    {
        public static string GetRawFileText(string path)
        {
            if (File.Exists(path))
            {
                using (StreamReader stream = File.OpenText(path))
                {
                    return stream.ReadToEnd();
                }
            }
            return "";
        }

        public static string RemoveFirstLine(string text)
        {
            string[] lines = text.Split(Environment.NewLine).Skip(1).ToArray();
            return string.Join(Environment.NewLine, lines);
        }
    }
}

