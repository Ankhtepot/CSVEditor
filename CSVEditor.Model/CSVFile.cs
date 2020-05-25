using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using CSVEditor.Services;
using JetBrains.Annotations;

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


        public ObservableCollection<ObservableCollection<string>> Lines;

        public ObservableCollection<CsvColumnConfiguration> ColumnConfigurations;

        private static List<char> delimiters = new List<char> { ',', ';', ':' };

        private static List<char> blockIdentifiers = new List<char> { '\"', '\'' };

        public CsvFile() : this("", 0, ',','"', new ObservableCollection<ObservableCollection<string>>())
        {
        }

        public CsvFile(string fullName, int headerIndex, char delimiter, char blockIdentifier, ObservableCollection<ObservableCollection<string>> lines)
        {
            AbsPath = fullName ?? throw new ArgumentNullException(nameof(fullName));
            HeaderIndex = headerIndex;
            HeadersStrings = new List<string>();
            Delimiter = delimiter;
            BlockIdentifier = blockIdentifier;
            ColumnCount = HeadersStrings?.Count ?? 0;
            Lines = lines ?? throw new ArgumentNullException(nameof(lines));
            ColumnConfigurations = new ObservableCollection<CsvColumnConfiguration>();
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

        public CsvFile(string absPath,
            int headerIndex,
            List<string> headersStrings,
            char delimiter,
            char blockIdentifier,
            int columnCount,
            ObservableCollection<ObservableCollection<string>> lines,
            ObservableCollection<CsvColumnConfiguration> columnConfigurations)
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

        public static void SetDelimiters(List<char> newDelimitersList)
        {
            delimiters = newDelimitersList;
        }

        public static void AddBlockIdentifier(char newBlockIdentifier)
        {
            blockIdentifiers.Add(newBlockIdentifier);
        }

        public static void SetBlockIdentifier(List<char> newBlockIdentifiers)
        {
            blockIdentifiers = newBlockIdentifiers;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public override bool Equals(object obj)
        {
            return obj is CsvFile file &&
                   AbsPath == file.AbsPath &&
                   HeaderIndex == file.HeaderIndex &&
                   EqualityComparer<List<string>>.Default.Equals(HeadersStrings, file.HeadersStrings) &&
                   Delimiter == file.Delimiter &&
                   BlockIdentifier == file.BlockIdentifier &&
                   ColumnCount == file.ColumnCount &&
                   EqualityComparer<ObservableCollection<ObservableCollection<string>>>.Default.Equals(Lines, file.Lines) &&
                   EqualityComparer<ObservableCollection<CsvColumnConfiguration>>.Default.Equals(ColumnConfigurations, file.ColumnConfigurations);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(AbsPath, HeaderIndex, HeadersStrings, Delimiter, BlockIdentifier, ColumnCount, Lines, ColumnConfigurations);
        }
    }
}
