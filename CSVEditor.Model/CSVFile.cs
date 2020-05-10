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


        private ObservableCollection<ObservableCollection<string>> Lines;

        public ObservableCollection<CsvColumnConfiguration> ColumnConfigurations;

        private static ObservableCollection<char> delimiters = new ObservableCollection<char> { ',', ';', ':' };

        private static ObservableCollection<char> blockIdentifiers = new ObservableCollection<char> { '\"', '\'' };

        private CsvFile() : this("", 0, ',', new ObservableCollection<ObservableCollection<string>>())
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
            var transformedFile = csvFileFromAbsPath(absFilePath);
            AbsPath = transformedFile.AbsPath;
            HeaderIndex = transformedFile.HeaderIndex;
            HeadersStrings = transformedFile.HeadersStrings;
            Delimiter = transformedFile.Delimiter;
            Lines = transformedFile.Lines;
            ColumnConfigurations = transformedFile.ColumnConfigurations;
        }

        private CsvFile csvFileFromAbsPath(string path)
        {
            CsvFile result = new CsvFile();

            if (File.Exists(path))
            {
                var text = FileProcessingServices.GetRawFileText(path);

                if (text == "") return result;

                string[] lines = text.Split(Environment.NewLine);

                var fileIsValid = true;

                try
                {
                    result.Delimiter = IdentifyCsvDelimiter(lines[0]);
                    result.HeadersStrings = lines[0].Split(result.Delimiter).ToList();                    
                }
                catch (InvalidDataException)
                {
                    fileIsValid = false;
                }

                if (fileIsValid)
                {
                    result.ColumnCount = result.HeadersStrings.Count;
                    result.AbsPath = path;

                    for (int i = 0; i < result.ColumnCount; i++)
                    {
                        result.ColumnConfigurations.Add(new CsvColumnConfiguration());
                    }

                    var csvLines = new ObservableCollection<ObservableCollection<string>>();
                    var columnContents = getColumnContents(FileProcessingServices.RemoveFirstLine(text), result.Delimiter);

                    while(columnContents.Count > 0)
                    {
                        ObservableCollection<string> newLine = new ObservableCollection<string>();
                        for (int i = 0; i < result.ColumnCount; i++)
                        {
                            newLine.Add(columnContents[0]);
                            columnContents.RemoveAt(0);
                        }
                        csvLines.Add(newLine);
                    }

                    result.Lines = csvLines;
                }
            }

            return result;
        }

        private ObservableCollection<string> getColumnContents(string text, char delimiter)
        {
            ObservableCollection<string> result = new ObservableCollection<string>();

            var workingText = text;

            while (workingText != string.Empty)
            {
                if (workingText[0] == delimiter)
                {
                    result.Add("");
                    workingText = workingText.Remove(0, 1);
                } 
                else
                if(blockIdentifiers.Contains(workingText[0]))
                {
                    var endOfBlockIndex = workingText.IndexOf(char.ToString(workingText[0]) + char.ToString(delimiter), 1) + 1;
                    result.Add(workingText.Substring(0, endOfBlockIndex));
                    workingText = workingText.Remove(0, endOfBlockIndex + 1);
                }
                else
                {
                    var delimiterIndex = workingText.IndexOf(delimiter);
                    var endOfLineIndex = workingText.IndexOf("\n");
                    delimiterIndex = endOfLineIndex != -1 && endOfLineIndex < delimiterIndex
                        ? endOfLineIndex
                        : delimiterIndex;
                    var newContent = delimiterIndex == -1 
                        ? workingText
                        : workingText.Substring(0, delimiterIndex);
                    result.Add(newContent);
                    workingText = workingText.Remove(0, delimiterIndex == -1 ? workingText.Length : delimiterIndex + 1);
                }
            }

            return result;
        }

        private char IdentifyCsvDelimiter(string line)
        {
            foreach (char letter in line)
            {
                if (!(char.IsLetterOrDigit(letter) || char.IsWhiteSpace(letter) || blockIdentifiers.Contains(letter)) && delimiters.Contains(letter))
                {
                    return letter;
                }
            }

            throw new InvalidDataException("Invalid CSV file.");
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
