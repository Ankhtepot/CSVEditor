using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using CSVEditor.Annotations;
using CSVEditor.Services;
using static CSVEditor.Model.Enums;

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
                absPath = value;
                OnPropertyChanged(nameof(AbsPath));
            }
        }

        private int headerIndex;

        public int HeaderIndex
        {
            get { return headerIndex; }
            set
            {
                headerIndex = value;
                OnPropertyChanged(nameof(HeaderIndex));
            }
        }

        private List<string> headersStrings;

        public List<string> HeadersStrings
        {
            get { return headersStrings; }
            set
            {
                headersStrings = value;
                OnPropertyChanged(nameof(HeadersStrings));
            }
        }

        private char delimiter;

        public char Delimiter
        {
            get { return delimiter; }
            set
            {
                delimiter = value;
                OnPropertyChanged(nameof(Delimiter));
            }
        }


        private ObservableCollection<ObservableCollection<string>> lines;

        public ObservableCollection<ObservableCollection<string>> Lines
        {
            get { return lines; }
            set
            {
                lines = value;
                OnPropertyChanged(nameof(Lines));
            }
        }

        private static ObservableCollection<char> delimiters = new ObservableCollection<char> { ',', ';', ':' };

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
        }

        public CsvFile(string absFilePath)
        {
            var transformedFile = csvFileFromAbsPath(absFilePath);
            AbsPath = transformedFile.AbsPath;
            HeaderIndex = transformedFile.HeaderIndex;
            HeadersStrings = transformedFile.HeadersStrings;
            Delimiter = transformedFile.Delimiter;
            Lines = transformedFile.Lines;
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
                }
                catch (InvalidDataException)
                {
                    fileIsValid = false;
                }

                if (fileIsValid)
                {
                    var csvLines = new ObservableCollection<ObservableCollection<string>>();

                    foreach (var line in lines)
                    {
                        csvLines.Add(getColumnContents(line, result.Delimiter));
                    }

                    result.headersStrings = csvLines[0].ToList();
                    csvLines.RemoveAt(0);
                    result.lines = csvLines;
                }
            }

            return result;
        }

        private ObservableCollection<string> getColumnContents(string line, char delimiter)
        {
            //TODO - add algorthytm to properly split "" and '' fields
            return new ObservableCollection<string>(line.Split(delimiter));
        }

        private char IdentifyCsvDelimiter(string line)
        {
            foreach (char letter in line)
            {
                if (!(char.IsLetterOrDigit(letter) || char.IsWhiteSpace(letter) || letter == '\"' || letter == '\'') && delimiters.Contains(letter))
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
