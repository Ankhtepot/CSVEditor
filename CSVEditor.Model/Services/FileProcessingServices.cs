using CSVEditor.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json;

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

        public static async Task<string> RemoveFirstLineAsync(string text)
        {
            string[] lines = await Task.Run(() => text.Split(Environment.NewLine).Skip(1).ToArray());
            return await Task.Run(() => string.Join(Environment.NewLine, lines));
        }

        public static CsvFile CsvFileFromAbsPath(string path, List<char> blockIdentifiers, List<char> delimiters, BackgroundWorker worker = null)
        {
            CsvFile result = new CsvFile();

            if (File.Exists(path))
            {
                var text = GetRawFileText(path);

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
                    ResolveCsvFileConfiguration(result);

                    var csvLines = new List<List<string>>();
                    var columnContents = GetColumnContents(RemoveFirstLineAsync(text).Result, result.Delimiter, blockIdentifiers, worker) ?? new ObservableCollection<string>();

                    while (columnContents?.Count > 0)
                    {
                        if (worker?.CancellationPending == true)
                        {
                            return null;
                        }

                        var newLine = new List<string>();
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

        private static void ResolveCsvFileConfiguration(CsvFile result)
        {
            for (int i = 0; i < result.ColumnCount; i++)
            {
                result.ColumnConfigurations.Add(new CsvColumnConfiguration());
            }
        }

        public static ObservableCollection<string> GetColumnContents(string text, char delimiter, List<char> blockIdentifiers, BackgroundWorker worker = null)
        {
            var result = new ObservableCollection<string>();

            var workingText = text;

            while (workingText != string.Empty)
            {
                if(worker?.CancellationPending == true)
                {
                    return null;
                }

                if (workingText[0] == delimiter)
                {
                    result.Add(""); // Adding empty string as there needs to be an record of empty content between delimiters in a resulting list.
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

        public static char IdentifyCsvDelimiter(string line, List<char> blockIdentifiers, List<char> delimiters)
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

