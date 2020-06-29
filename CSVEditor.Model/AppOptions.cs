﻿using System.Collections.Generic;

namespace CSVEditor.Model
{
    public class AppOptions
    {
        public string LastRootPath { get; set; }
        public string LastSelectedFilePath { get; set; }
        public CsvFile LastSelectedCsvFile { get; set; }
        public CsvFile LastLoadedUneditedCsvFile { get; set; }
        public VisualConfig VisualConfig { get; set; }
        public List<char> Delimiters { get; set; }
        public List<char> BlockIdentifiers { get; set; }
        public List<DirectoryWithCsv> LastCsvFilesStructure { get; set; }

        public AppOptions() : this(
            "",
            "",
            null,
            null,
            new VisualConfig(),
            CsvFile.Delimiters,
            CsvFile.BlockIdentifiers,
            new List<DirectoryWithCsv>() { new DirectoryWithCsv() })
        {
        }

        public AppOptions(
            string lastRootPath,
            string lastSelectedFilePath,
            CsvFile lastSelectedCsvFile,
            CsvFile lastLoadedUneditedCsvFile,
            VisualConfig visulaConfig,
            List<char> delimiters,
            List<char> blockIdentifiers,
            List<DirectoryWithCsv> lastCsvFilesStructure)
        {
            LastRootPath = lastRootPath;
            LastSelectedFilePath = lastSelectedFilePath;
            LastSelectedCsvFile = lastSelectedCsvFile;
            LastLoadedUneditedCsvFile = lastLoadedUneditedCsvFile;
            VisualConfig = visulaConfig;
            Delimiters = delimiters;
            BlockIdentifiers = blockIdentifiers;
            LastCsvFilesStructure = lastCsvFilesStructure;
        }
    }
}
