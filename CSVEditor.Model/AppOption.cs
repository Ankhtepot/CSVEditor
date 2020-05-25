using System;
using System.Collections.Generic;
using System.Text;

namespace CSVEditor.Model
{
    public class AppOption
    {
        public string LastRootPath;
        public string LastSelectedFilePath;
        public CsvFile LastSelectedCsvFile;
        public List<string> Delimiters;
        public List<string> BlockIdentifiers;
    }
}
