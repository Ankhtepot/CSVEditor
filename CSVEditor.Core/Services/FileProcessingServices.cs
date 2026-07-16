using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSVEditor.Core.Extensions;
using CSVEditor.Core.HelperClasses;

namespace CSVEditor.Core.Services
{
    public static class FileProcessingServices
    {
        public static string GetRawFileText(string path)
        {
            if (!File.Exists(path))
            {
                return "";
            }

            using StreamReader stream = File.OpenText(path);
            return stream.ReadToEnd();
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
            CsvFile result = new();

            if (File.Exists(path))
            {
                string text = GetRawFileText(path).Replace("\r", "");

                if (text == "") return result;

                string[] lines = text.Split(Environment.NewLine);
                string firstLine = text.Substring(0, text.IndexOf('\n'));

                bool fileIsValid = true;

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

                    List<List<string>> csvLines = new();
                    List<string> columnContents = GetColumnContents(RemoveFirstLineAsync(text, firstLine).Result, result.Delimiter, blockIdentifiers, worker) ?? new List<string>();

                    while (columnContents.Count > 0)
                    {
                        if (worker?.CancellationPending == true)
                        {
                            return null;
                        }

                        List<string> newLine = new();
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
                        FillUpLastLineIfNecessary(csvLines[^1], result.ColumnCount);
                    }

                    result.Lines = csvLines;
                }
            }

            return result;
        }

        private static void FillUpLastLineIfNecessary(List<string> list, int columnCount)
        {
            while (true)
            {
                if (list.Count >= columnCount) return;

                list.Add("<added extra>");
            }
        }

        public static List<string> GetColumnContents(string text, char delimiter, List<char> blockIdentifiers, BackgroundWorker worker = null)
        {
            List<string> result = new();

            string workingText = text.Replace("\r", "");

            while (workingText != string.Empty)
            {
                if (worker?.CancellationPending == true)
                {
                    return null;
                }

                if (workingText[0] == delimiter)
                {
                    result.Add(""); // Adding empty string as there needs to be a record of empty content between delimiters in a resulting list.
                    workingText = workingText.Remove(0, 1);
                }
                else
                if (blockIdentifiers.Contains(workingText[0]))
                {
                    int endOfBlockIndex = workingText.IndexOf(char.ToString(workingText[0]) + char.ToString(delimiter), 1, StringComparison.Ordinal) + 1;
                    int endOfTheLine = workingText.IndexOf(char.ToString(workingText[0]) + "\n", 1, StringComparison.Ordinal) + 1;

                    endOfBlockIndex = endOfBlockIndex > endOfTheLine && endOfTheLine != 0 ? endOfTheLine : endOfBlockIndex;

                    if (endOfBlockIndex == 0) // this signifies end of the line
                    {
                        endOfBlockIndex = workingText.LastIndexOf(char.ToString(workingText[0]) + Environment.NewLine, StringComparison.Ordinal);

                        if (endOfBlockIndex != -1)
                        {
                            continue;
                        }

                        result.Add(workingText.Substring(1, workingText.Length - 2)); //to not include blockIdentifier in text
                        workingText = "";
                    }
                    else
                    {
                        result.Add(workingText.Substring(1, endOfBlockIndex - 2)); //to not include blockIdentifier in text
                        workingText = workingText.Remove(0, endOfBlockIndex + 1);
                    }
                }
                else
                {
                    int delimiterIndex = workingText.IndexOf(delimiter);
                    int endOfLineIndex = workingText.IndexOf('\n', StringComparison.Ordinal);
                    delimiterIndex = endOfLineIndex != -1 && endOfLineIndex < delimiterIndex
                        ? endOfLineIndex
                        : delimiterIndex;
                    string newContent = delimiterIndex == -1
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
            StringBuilder stringBuilder = new();

            for (int i = 0; i < line.Count; i++)
            {
                stringBuilder.Append(
                    line[i].ContainsAny(new[] {delimiter, blockIdentifier, '-', ':', '\\', '.', ',', ';', '&', '\''})
                        ? $"{blockIdentifier}{line[i]}{blockIdentifier}"
                        : line[i]);

                if (i < line.Count - 1)
                {
                    stringBuilder.Append(delimiter);
                }
            }

            return stringBuilder.ToString();
        }

        public static string HeadersLineToStringLine(CsvFile csvFile)
        {
            StringBuilder stringBuilder = new();

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

