using System.Collections.Generic;
using CSVEditor.Model.HelperClasses;

namespace CSVEditor.Model
{
    public class AppOptions
    {
        public string LastRootPath { get; set; }
        public string LastSelectedFilePath { get; set; }
        public bool WasEdited { get; set; }
        public CsvFile LastSelectedCsvFile { get; set; }
        public VisualConfig VisualConfig { get; set; }
        public SaveOptions SaveOptions { get; set; }
        public GitOptions GitOptions { get; set; }
        public List<char> Delimiters { get; set; }
        public List<char> BlockIdentifiers { get; set; }
        public List<DirectoryWithCsv> LastCsvFilesStructure { get; set; }

        public AppOptions() : this(
            "",
            "",
            false,
            null,
            new VisualConfig(),
            new SaveOptions(),
            new GitOptions(),
            CsvFile.Delimiters,
            CsvFile.BlockIdentifiers,
            new List<DirectoryWithCsv>() { new DirectoryWithCsv() })
        {
        }

        public AppOptions(
            string lastRootPath,
            string lastSelectedFilePath,
            bool wasEdited,
            CsvFile lastSelectedCsvFile,
            VisualConfig visulaConfig,
            SaveOptions saveOptions,
            GitOptions gitOptions,
            List<char> delimiters,
            List<char> blockIdentifiers,
            List<DirectoryWithCsv> lastCsvFilesStructure)
        {
            LastRootPath = lastRootPath;
            LastSelectedFilePath = lastSelectedFilePath;
            WasEdited = wasEdited;
            LastSelectedCsvFile = lastSelectedCsvFile;
            VisualConfig = visulaConfig;
            SaveOptions = saveOptions;
            GitOptions = gitOptions;
            Delimiters = delimiters;
            BlockIdentifiers = blockIdentifiers;
            LastCsvFilesStructure = lastCsvFilesStructure;
        }
    }
}
