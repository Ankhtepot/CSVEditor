using System;
using System.Collections.Generic;
using System.Text;

namespace CSVEditor.Model
{
    public class AppOptions
    {
        public string LastRootPath { get; set; }
        public string LastSelectedFilePath { get; set; }
        public CsvFile LastSelectedCsvFile { get; set; }
        public List<char> Delimiters { get; set; }
        public List<char> BlockIdentifiers { get; set; }

        public AppOptions() : this("","",new CsvFile(), CsvFile.Delimiters, CsvFile.BlockIdentifiers)
        {
        }

        public AppOptions(string lastRootPath, string lastSelectedFilePath, CsvFile lastSelectedCsvFile, List<char> delimiters, List<char> blockIdentifiers)
        {
            LastRootPath = lastRootPath;
            LastSelectedFilePath = lastSelectedFilePath;
            LastSelectedCsvFile = lastSelectedCsvFile;
            Delimiters = delimiters;
            BlockIdentifiers = blockIdentifiers;
        }
    }
}
