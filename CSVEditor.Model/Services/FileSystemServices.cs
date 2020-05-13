using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
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
            List<string> rootPathDirectories = new List<string>();

            try
            {
                rootPathDirectories = Directory.GetDirectories(rootPath, @".git").ToList();
            }
            catch (Exception e)
            {
                Console.WriteLine("Error while checking for GitRepo: " + e.Message);
            }

            if (rootPathDirectories != null && rootPathDirectories.Count > 0)
            {
                foreach (var directory in rootPathDirectories)
                {
                    if (Regex.Match(directory, "\\.git").Success)
                    {
                        return true;
                    }
                } 
            }

            return false;
        }

        public static List<string> GetDirectoriesFromRootPath(string rootPath)
        {
            if (!Directory.Exists(rootPath))
            {
                return null;
            }

            IEnumerable<string> result = getDirectoriesFromPath(rootPath);

            foreach (var directory in result.Where(dir => !Regex.Match(dir, "\\.git").Success))
            {
                //Console.WriteLine("Scanning directory: " + directory);
                result = result.Concat(GetDirectoriesFromRootPath(directory)).ToList();
            }

            return result.ToList();
        }

        private static List<string> getDirectoriesFromPath(string path)
        {
            List<string> directories = new List<string>();

            try
            {
                directories = Directory.GetDirectories(path).ToList();
            }
            catch (UnauthorizedAccessException e)
            {
                Console.WriteLine($"Insufficient rights to scan direcotry at \"{path}\".");
            }
            //catch (Exception e)
            //{
            //    Console.WriteLine($"Uknown error occured while reading directories at \"{path}\".");
            //}

            return directories;
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
            //catch (Exception e)
            //{
            //    Console.WriteLine($"Unexpected error occured: {e.Message}");
            //}

            var directoryPath = Regex.Replace(path, Regex.Escape(rootPath), ".");
            directoryPath = directoryPath == "." ? Constants.ROOT_DIRECTORY : directoryPath;

            return (csvFiles != null && csvFiles.Count > 0) 
                ? new DirectoryWithCsv(directoryPath, csvFiles) 
                : null;
        }
    }
}
