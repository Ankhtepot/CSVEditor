using System.Collections.Generic;
using CSVEditor.Model.HelperClasses;

namespace CSVEditor.Model
{
    public class AppOptions
    {
        public string LastRootPath { get; set; }
        public string LastSelectedFilePath { get; set; }
        public CsvFile LastSelectedCsvFile { get; set; }
        public VisualConfig VisualConfig { get; set; }
        public SaveOptions SaveOptions { get; set; }
        public List<char> Delimiters { get; set; }
        public List<char> BlockIdentifiers { get; set; }
        public List<DirectoryWithCsv> LastCsvFilesStructure { get; set; }

        public AppOptions() : this(
            "",
            "",
            null,
            new VisualConfig(),
            new SaveOptions(),
            CsvFile.Delimiters,
            CsvFile.BlockIdentifiers,
            new List<DirectoryWithCsv>() { new DirectoryWithCsv() })
        {
        }

        public AppOptions(
            string lastRootPath,
            string lastSelectedFilePath,
            CsvFile lastSelectedCsvFile,
            VisualConfig visulaConfig,
            SaveOptions saveOptions,
            List<char> delimiters,
            List<char> blockIdentifiers,
            List<DirectoryWithCsv> lastCsvFilesStructure)
        {
            LastRootPath = lastRootPath;
            LastSelectedFilePath = lastSelectedFilePath;
            LastSelectedCsvFile = lastSelectedCsvFile;
            VisualConfig = visulaConfig;
            SaveOptions = saveOptions;
            Delimiters = delimiters;
            BlockIdentifiers = blockIdentifiers;
            LastCsvFilesStructure = lastCsvFilesStructure;
        }
    }
}
