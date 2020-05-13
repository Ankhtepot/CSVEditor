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
            set
            {
                absPath = value; OnPropertyChanged(nameof(AbsPath));
            }
        }

        private int headerIndex;

        public int HeaderIndex
        {
            get { return headerIndex; }
            set
            {
                headerIndex = value;OnPropertyChanged(nameof(HeaderIndex));
            }
        }

        private List<string> headersStrings;

        public List<string> HeadersStrings
        {
            get { return headersStrings; }
            set
            {
                headersStrings = value;OnPropertyChanged(nameof(HeadersStrings));
            }
        }

        private char delimiter;

        public char Delimiter
        {
            get { return delimiter; }
            set
            {
                delimiter = value; OnPropertyChanged(nameof(Delimiter));
            }
        }

        private int columnCount;

        public int ColumnCount
        {
            get { return columnCount; }
            set 
            {
                columnCount = value; OnPropertyChanged(nameof(ColumnCount));
            }
        }


        public ObservableCollection<ObservableCollection<string>> Lines;

        public ObservableCollection<CsvColumnConfiguration> ColumnConfigurations;

        private static ObservableCollection<char> delimiters = new ObservableCollection<char> { ',', ';', ':' };

        private static ObservableCollection<char> blockIdentifiers = new ObservableCollection<char> { '\"', '\'' };

        public CsvFile() : this("", 0, ',', new ObservableCollection<ObservableCollection<string>>())
        {
        }

        public CsvFile(string fullName, int headerIndex, char delimiter, ObservableCollection<ObservableCollection<string>> lines)
        {
            AbsPath = fullName ?? throw new ArgumentNullException(nameof(fullName));
            HeaderIndex = headerIndex;
            HeadersStrings = new List<string>();
            Delimiter = delimiter;
            Lines = lines ?? throw new ArgumentNullException(nameof(lines));
            ColumnConfigurations = new ObservableCollection<CsvColumnConfiguration>();
        }

        public CsvFile(string absFilePath)
        {
            var transformedFile = FileProcessingServices.CsvFileFromAbsPath(absFilePath, blockIdentifiers, delimiters);
            AbsPath = transformedFile.AbsPath;
            HeaderIndex = transformedFile.HeaderIndex;
            HeadersStrings = transformedFile.HeadersStrings;
            Delimiter = transformedFile.Delimiter;
            Lines = transformedFile.Lines;
            ColumnConfigurations = transformedFile.ColumnConfigurations;
        }

        public static void addDelimiter(char newDelimiter)
        {
            delimiters.Add(newDelimiter);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
