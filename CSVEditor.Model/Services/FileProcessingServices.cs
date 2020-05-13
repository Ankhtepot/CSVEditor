using CSVEditor.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;

namespace CSVEditor.Services
{
    public class FileProcessingServices
    {
        public static string GetRawFileText(string path)
        {
            if (File.Exists(path))
            {
                using (StreamReader stream = File.OpenText(path))
                {
                    return stream.ReadToEnd();
                }
            }
            return "";
        }

        public static string RemoveFirstLine(string text)
        {
            string[] lines = text.Split(Environment.NewLine).Skip(1).ToArray();
            return string.Join(Environment.NewLine, lines);
        }

        public static CsvFile CsvFileFromAbsPath(string path, ObservableCollection<char> blockIdentifiers, ObservableCollection<char> delimiters)
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
                    result.Delimiter = IdentifyCsvDelimiter(lines[0], blockIdentifiers, delimiters);
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
                    var columnContents = GetColumnContents(FileProcessingServices.RemoveFirstLine(text), result.Delimiter, blockIdentifiers);

                    while (columnContents.Count > 0)
                    {
                        ObservableCollection<string> newLine = new ObservableCollection<string>();
                        for (int i = 0; i < result.ColumnCount; i++)
                        {
                            if (columnContents.Count > 0)
                            {
                                newLine.Add(columnContents[0]);
                                columnContents.RemoveAt(0);
                            } 
                            else
                            {
                                break;
                            }
                        }
                        csvLines.Add(newLine);
                    }

                    result.Lines = csvLines;
                }
            }            

            return result;
        }

        public static ObservableCollection<string> GetColumnContents(string text, char delimiter, ObservableCollection<char> blockIdentifiers)
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
                if (blockIdentifiers.Contains(workingText[0]))
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

        public static char IdentifyCsvDelimiter(string line, ObservableCollection<char> blockIdentifiers, ObservableCollection<char> delimiters)
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
    }
}

