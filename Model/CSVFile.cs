using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using CSVEditor.Annotations;
using static CSVEditor.Model.Enums;

namespace CSVEditor.Model
{
    public class CsvFile : INotifyPropertyChanged
    {
        private string absPath;

        public string AbsPath {
            get { return absPath; }
            set
            {
                absPath = value;
                OnPropertyChanged(nameof(AbsPath));
            }
        }

        private int headerIndex;

        public int HeaderIndex {
            get { return headerIndex; }
            set
            {
                headerIndex = value;
                OnPropertyChanged(nameof(HeaderIndex));
            }
        }

        private List<string> headersStrings;

        public List<string> HeadersStrings {
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


        private ObservableCollection<ObservableCollection<CsvField>> lines;

        public ObservableCollection<ObservableCollection<CsvField>> Lines {
            get { return lines; }
            set
            {
                lines = value;
                OnPropertyChanged(nameof(Lines));
            }
        }

        private static ObservableCollection<char> delimiters = new ObservableCollection<char> { ',', ';',':'};

        private CsvFile() : this ("", 0, ',', new ObservableCollection<ObservableCollection<CsvField>>())
        {
        }

        public CsvFile(string fullName, int headerIndex, char delimiter, ObservableCollection<ObservableCollection<CsvField>> lines)
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

            if(File.Exists(path))
            {
                using(StreamReader stream = File.OpenText(path))
                {
                    string text =  stream.ReadToEnd();
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
                        var csvLines = new ObservableCollection<ObservableCollection<CsvField>>();

                        foreach (var line in lines)
                        {
                            var csvLine = new ObservableCollection<CsvField>();
                            //TODO: ignore part of text in "" or ''
                            foreach (var column in line.Split(result.Delimiter))
                            {
                                var field = new CsvField(FieldType.TextBlock, column, "");
                                csvLine.Add(field);
                            }

                            csvLines.Add(csvLine);
                        } 
                    }
                }
            }

            return result;
        }

        private char IdentifyCsvDelimiter(string line)
        {
            //TODO: find to way to skip "" and '' strings to determinate delimiter
            foreach(char letter in line)
            {
                if (!(char.IsLetterOrDigit(letter) || char.IsWhiteSpace(letter) || letter == '\"' || letter == '\'')) 
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
