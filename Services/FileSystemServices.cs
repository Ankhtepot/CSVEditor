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
            //TODO: handle GetDirectories exception
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
            var result = CrawlDirectoryForCsvFiles(new ObservableCollection<DirectoryWithCsv>(), rootPath, rootPath);
            return result;
        }

        public static ObservableCollection<DirectoryWithCsv> CrawlDirectoryForCsvFiles(ObservableCollection<DirectoryWithCsv> resultCollection, string rootPath, string path)
        {
            var directoryCsvFiles = ScanDirectory(rootPath, path);

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
                CrawlDirectoryForCsvFiles(resultCollection, rootPath, directory);
            }

            return resultCollection;
        }

        public static DirectoryWithCsv ScanDirectory(string rootPath, string path)
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

            var directoryPath = Regex.Replace(path, Regex.Escape(rootPath), ".");
            directoryPath = directoryPath == "." ? Constants.ROOT_DIRECTORY : directoryPath;

            return (csvFiles != null && csvFiles.Count > 0) 
                ? new DirectoryWithCsv(directoryPath, csvFiles) 
                : null;
        }
    }
}
