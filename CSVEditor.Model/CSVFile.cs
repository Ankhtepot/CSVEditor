﻿using CSVEditor.Model.HelperClasses;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using CSVEditor.Model.Services;

namespace CSVEditor.Model
{
    public class CsvFile : INotifyPropertyChanged
    {
        private string absPath;
        public string AbsPath
        {
            get { return absPath; }
            set { absPath = value; OnPropertyChanged(); }
        }

        private int headerIndex;
        public int HeaderIndex
        {
            get { return headerIndex; }
            set { headerIndex = value; OnPropertyChanged(); }
        }

        private List<string> headersStrings;
        public List<string> HeadersStrings
        {
            get { return headersStrings; }
            set { headersStrings = value; OnPropertyChanged(); }
        }

        private char delimiter;
        public char Delimiter
        {
            get { return delimiter; }
            set { delimiter = value; OnPropertyChanged(); }
        }

        private char blockIdentifier;
        public char BlockIdentifier
        {
            get { return blockIdentifier; }
            set { blockIdentifier = value; OnPropertyChanged(); }
        }

        private int columnCount;
        public int ColumnCount
        {
            get { return columnCount; }
            set { columnCount = value; OnPropertyChanged(); }
        }

        private List<List<string>> lines;
        public List<List<string>> Lines
        {
            get { return lines; }
            set { lines = value; OnPropertyChanged(); }
        }

        private List<CsvColumnConfiguration> columnConfigurations;
        public List<CsvColumnConfiguration> ColumnConfigurations
        {
            get { return columnConfigurations; }
            set { columnConfigurations = value; OnPropertyChanged(); }
        }

        private static List<char> delimiters = new List<char> { ',', ';', ':'};
        public static List<char> Delimiters
        {
            get { return delimiters; }
            set { delimiters = value; }
        }

        private static List<char> blockIdentifiers = new List<char> { '\"', '\''};
        public static List<char> BlockIdentifiers
        {
            get { return blockIdentifiers; }
            set { blockIdentifiers = value; }
        }

        public CsvFile() : this("", 0, ',','"', new List<List<string>>())
        {
        }

        public CsvFile(string fullName, int headerIndex, char delimiter, char blockIdentifier, List<List<string>> lines)
        {
            AbsPath = fullName ?? throw new ArgumentNullException(nameof(fullName));
            HeaderIndex = headerIndex;
            HeadersStrings = new List<string>();
            Delimiter = delimiter;
            BlockIdentifier = blockIdentifier;
            ColumnCount = HeadersStrings?.Count ?? 0;
            Lines = lines ?? throw new ArgumentNullException(nameof(lines));
            ColumnConfigurations = new List<CsvColumnConfiguration>();
        }

        public CsvFile(string absFilePath, BackgroundWorker worker = null)
        {
            var transformedFile = FileProcessingServices.CsvFileFromAbsPath(absFilePath, blockIdentifiers, delimiters, worker);
            AbsPath = transformedFile.AbsPath;
            HeaderIndex = transformedFile.HeaderIndex;
            HeadersStrings = transformedFile.HeadersStrings;
            Delimiter = transformedFile.Delimiter;
            BlockIdentifier = transformedFile.BlockIdentifier;
            ColumnCount = transformedFile.ColumnCount;
            Lines = transformedFile.Lines;
            ColumnConfigurations = transformedFile.ColumnConfigurations;
        }

        public CsvFile(CsvFile originalCsvFile)
        {
            AbsPath = originalCsvFile.AbsPath;
            HeaderIndex = originalCsvFile.HeaderIndex;
            HeadersStrings = new List<string>(originalCsvFile.HeadersStrings);
            Delimiter = originalCsvFile.Delimiter;
            BlockIdentifier = originalCsvFile.BlockIdentifier;
            ColumnCount = originalCsvFile.ColumnCount;
            Lines = new List<List<string>>(originalCsvFile.Lines);
            ColumnConfigurations = new List<CsvColumnConfiguration>(originalCsvFile.ColumnConfigurations);
        }

        public CsvFile(string absPath,
            int headerIndex,
            List<string> headersStrings,
            char delimiter,
            char blockIdentifier,
            int columnCount,
            List<List<string>> lines,
            List<CsvColumnConfiguration> columnConfigurations)
        {
            AbsPath = absPath ?? throw new ArgumentNullException(nameof(absPath));
            HeaderIndex = headerIndex;
            HeadersStrings = headersStrings ?? throw new ArgumentNullException(nameof(headersStrings));
            Delimiter = delimiter;
            BlockIdentifier = blockIdentifier;
            ColumnCount = columnCount;
            Lines = lines ?? throw new ArgumentNullException(nameof(lines));
            ColumnConfigurations = columnConfigurations ?? throw new ArgumentNullException(nameof(columnConfigurations));
        }

        public static CsvFile Copy(CsvFile other)
        {
            if (other == null || other !is CsvFile)
            {
                return null;
            }

            return new CsvFile()
            {
                AbsPath = other.AbsPath,
                HeaderIndex = other.HeaderIndex,
                HeadersStrings = other.HeadersStrings,
                Delimiter = other.Delimiter,
                BlockIdentifier = other.BlockIdentifier,
                ColumnCount = other.ColumnCount,
                Lines = other.Lines,
                ColumnConfigurations = other.ColumnConfigurations
            };
        }

        public static void AddDelimiter(char newDelimiter)
        {
            delimiters.Add(newDelimiter);
        }

        public static void AddBlockIdentifier(char newBlockIdentifier)
        {
            blockIdentifiers.Add(newBlockIdentifier);
        }

        public override bool Equals(object obj)
        {
            var result = obj is CsvFile file &&
                   AbsPath == file.AbsPath &&
                   HeaderIndex == file.HeaderIndex &&
                   EqualityComparer<List<string>>.Default.Equals(HeadersStrings, file.HeadersStrings) &&
                   Delimiter == file.Delimiter &&
                   BlockIdentifier == file.BlockIdentifier &&
                   ColumnCount == file.ColumnCount &&
                   EqualityComparer<List<List<string>>>.Default.Equals(Lines, file.Lines) &&
                   EqualityComparer<List<CsvColumnConfiguration>>.Default.Equals(ColumnConfigurations, file.ColumnConfigurations);

            return result;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(AbsPath, HeaderIndex, HeadersStrings, Delimiter, BlockIdentifier, ColumnCount, Lines, ColumnConfigurations);
        }        

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }       
    }
}
