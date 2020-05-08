using System;
using System.Collections.Generic;
using System.IO;
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
    }
}

