using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using CSVEditor.Model;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace CSVEditor.ViewModel
{
    public class FileSystemServices
    {
        public static string LoadRootRepositoryPath()
        {
            var dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;
            var result = dialog.ShowDialog();

            if (result == CommonFileDialogResult.Ok)
            {
                return dialog.FileName;
            }
            else
            {
                return Constants.LOAD_REPOSITORY_FAILED;
            }
        }

        public static bool IsDirectoryWithGitRepository(string rootPath)
        {
            foreach (var directory in Directory.GetDirectories(rootPath))
            {
                if (Regex.Match(directory, "\\.git").Success)
                {
                    return true;
                }
            }

            return false;
        }

        public static ObservableCollection<DirectoryWithCsv> GetCsvFilesStructureFromRootDirectory(string rootPath)
        {
            var result = CrawlRootDirectory(new ObservableCollection<DirectoryWithCsv>(), rootPath);
            return result;
        }

        public static ObservableCollection<DirectoryWithCsv> CrawlRootDirectory(ObservableCollection<DirectoryWithCsv> resultCollection, string path)
        {
            var directoryCsvFiles = ScanDirectory(path);

            if (directoryCsvFiles != null)
            {
                resultCollection.Add(directoryCsvFiles);
            }

            IEnumerable<string> directories = new List<string>();

            try
            {
                directories = Directory.GetDirectories(path);
            }
            catch (UnauthorizedAccessException e)
            {
                Console.WriteLine($"Insufficient rights to scan CSV files in \"{path}\".");
            }

            foreach (var directory in directories.Where(dir => !Regex.Match(dir, "\\.git").Success))
            {
                //Console.WriteLine("Scanning directory: " + path);
                CrawlRootDirectory(resultCollection, directory);
            }

            return resultCollection;
        }

        public static DirectoryWithCsv ScanDirectory(string path)
        {
            ObservableCollection<string> csvFiles = null;
            try
            {
                csvFiles = new ObservableCollection<string>(
                       Directory.GetFiles(path, "*.csv", SearchOption.TopDirectoryOnly));
            }
            catch (UnauthorizedAccessException)
            {
                Console.WriteLine($"Insufficient rights to read all files in \"{path}\".");
            }

            return (csvFiles != null && csvFiles.Count > 0) 
                ? new DirectoryWithCsv(path, csvFiles) 
                : null;
        }
    }
}
