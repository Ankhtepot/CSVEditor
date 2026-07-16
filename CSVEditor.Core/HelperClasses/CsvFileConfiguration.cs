using CSVEditor.Core.HelperClasses;
using System.Collections.Generic;
using System.IO;

namespace CSVEditor.Core
{
    /// <summary>
    /// Class describing CSv File Configuration, intended to be used for serialization
    /// </summary>
    public class CsvFileConfiguration
    {
        public List<CsvColumnConfiguration> ColumnConfigurations { get; set; }

        public string AbsoluteFilePath { get; set; }

        public string FileName =>
            !string.IsNullOrEmpty(AbsoluteFilePath) && File.Exists(AbsoluteFilePath)
                ? Path.GetFileName(AbsoluteFilePath)
                : "";
    }
}
