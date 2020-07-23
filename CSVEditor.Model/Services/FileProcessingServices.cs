using CSVEditor.Model;
using CSVEditor.Model.HelperClasses;
using CSVEditor.Model.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public static async Task<string> RemoveFirstLineAsync(string text, string firstLine)
        {
            return await Task.Run(() =>
            {
                text = text.Replace(firstLine + '\n', "");

                if (text[0] == '\n' || text[0] == '\r')
                {
                    text.Remove(0);
                }

                return text;
            });
        }

        public static CsvFile CsvFileFromAbsPath(
            string path,
            List<char> blockIdentifiers,
            List<char> delimiters,
            BackgroundWorker worker = null)
        {
            CsvFile result = new CsvFile();

            if (File.Exists(path))
            {
                var text = GetRawFileText(path).Replace("\r", "");

                if (text == "") return result;

                string[] lines = text.Split(Environment.NewLine);
                string firstLine = text.Substring(0, text.IndexOf('\n'));

                var fileIsValid = true;

                try
                {
                    result.Delimiter = IdentifyCsvDelimiter(lines[0], blockIdentifiers, delimiters);
                    result.HeadersStrings = firstLine.Split(result.Delimiter).ToList();
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

                    var csvLines = new List<List<string>>();
                    var columnContents = GetColumnContents(RemoveFirstLineAsync(text, firstLine).Result, result.Delimiter, blockIdentifiers, worker) ?? new List<string>();

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

                    if (csvLines.Count > 0)
                    {
                        FillUpLastLineIfNecesarry(csvLines[csvLines.Count - 1], result.ColumnCount);
                    }

                    result.Lines = csvLines;
                }
            }

            return result;
        }

        private static void FillUpLastLineIfNecesarry(List<string> list, int columnCount)
        {
            if (list.Count < columnCount)
            {
                list.Add("<added extra>");
                FillUpLastLineIfNecesarry(list, columnCount);
            }
        }

        public static List<string> GetColumnContents(string text, char delimiter, List<char> blockIdentifiers, BackgroundWorker worker = null)
        {
            var result = new List<string>();

            var workingText = text.Replace("\r", "");

            while (workingText != string.Empty)
            {
                if (worker?.CancellationPending == true)
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
                    var endOfTheLine = workingText.IndexOf(char.ToString(workingText[0]) + "\n", 1) + 1;

                    endOfBlockIndex = endOfBlockIndex > endOfTheLine && endOfTheLine != 0 ? endOfTheLine : endOfBlockIndex;

                    if (endOfBlockIndex == 0) // this signifies end of the line
                    {
                        endOfBlockIndex = workingText.LastIndexOf(char.ToString(workingText[0]) + Environment.NewLine);

                        if (endOfBlockIndex == -1) //this signifies the end of the file
                        {
                            result.Add(workingText.Substring(1, workingText.Length - 2)); //to not include blockIdentifier in text
                            workingText = "";
                        }
                    }
                    else
                    {
                        result.Add(workingText.Substring(1, endOfBlockIndex - 2)); //to not include blockIdentifier in text
                        workingText = workingText.Remove(0, endOfBlockIndex + 1);
                    }
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

                    if (newContent.LastChar() == '\n')
                    {
                        newContent = newContent.Remove(newContent.Length - 1, 1);
                    }

                    result.Add(newContent);
                    workingText = workingText.Remove(0, delimiterIndex == -1 ? workingText.Length : delimiterIndex + 1);
                }
            }

            return result;
        }

        public static string CsvLineToString(List<string> line, char delimiter, char blockIdentifier)
        {
            var stringBuilder = new StringBuilder();

            for (int i = 0; i < line.Count; i++)
            {
                if (line[i].ContainsAny(new char[] { delimiter, blockIdentifier, '-', ':', '\\', '.', ',', ';', '&', '\'' }))
                {
                    stringBuilder.Append($"{blockIdentifier}{line[i]}{blockIdentifier}");
                }
                else
                {
                    stringBuilder.Append(line[i]);
                }

                if (i < line.Count - 1)
                {
                    stringBuilder.Append(delimiter);
                }
            }

            return stringBuilder.ToString();
        }

        public static string HeadersLineToStringLine(CsvFile csvFile)
        {
            var stringBuilder = new StringBuilder();

            for (int i = 0; i < csvFile.HeadersStrings.Count; i++)
            {
                if (i < csvFile.HeadersStrings.Count - 1)
                {
                    stringBuilder.Append($"{csvFile.HeadersStrings[i]}{csvFile.Delimiter}");
                }
                else // for last header string
                {
                    stringBuilder.Append($"{csvFile.HeadersStrings[i]}{Environment.NewLine}");
                }
            }

            return stringBuilder.ToString();
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

