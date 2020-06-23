﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Media.Imaging;
using CSVEditor.Model;
using CSVEditor.Model.Services;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace CSVEditor.ViewModel
{
    public class FileSystemServices
    {
        private static readonly string[] BASE_IMAGE_FILE_EXTENSIONS = { ".png", ".jpg", ".jpeg" };

        public static string QueryUserForRootRepositoryPath(string title = "")
        {
            var dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;
            var result = dialog.ShowDialog();

            if (!string.IsNullOrEmpty(title))
            {
                dialog.Title = title;
            }

            if (result == CommonFileDialogResult.Ok)
            {
                return dialog.FileName;
            }
            else
            {
                return Constants.LOAD_REPOSITORY_FAILED;
            }
        }

        public static string QueryUserForPath(string initialDirectory = "", string title = "")
        {
            var dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;

            if(!string.IsNullOrEmpty(title))
            {
                dialog.Title = title;
            }

            if (!string.IsNullOrEmpty(initialDirectory) && Directory.Exists(initialDirectory))
            {
                dialog.InitialDirectory = initialDirectory;
            }

            CommonFileDialogResult result = CommonFileDialogResult.None;
            try
            {
                result = dialog.ShowDialog();
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error while reading directory path: {e.Message}");
                return null;
            }

            if (result == CommonFileDialogResult.Ok)
            {
                return dialog.FileName;
            }
            else
            {
                return null;
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

        public static List<string> GetDirectoriesFromRootPath(string rootPath, BackgroundWorker worker = null)
        {
            if (!Directory.Exists(rootPath))
            {
                return null;
            }

            IEnumerable<string> directories = getDirectoriesFromPath(rootPath, worker) ?? new List<string>();

            foreach (var directory in directories.Where(dir => !Regex.Match(dir, "\\.git").Success))
            {
                //Console.WriteLine("Scanning directory: " + directory);
                var recursiveYield = GetDirectoriesFromRootPath(directory, worker);
                directories = recursiveYield != null
                    ? directories.Concat(recursiveYield).ToList()
                    : null;
            }

            return directories != null ? directories.ToList() : null;
        }

        private static List<string> getDirectoriesFromPath(string path, BackgroundWorker worker = null)
        {
            if (worker != null && worker.CancellationPending)
            {
                return null;
            }

            List<string> directories = new List<string>();

            try
            {
                directories = Directory.GetDirectories(path).ToList();
            }
            catch (UnauthorizedAccessException e)
            {
                Console.WriteLine($"Insufficient rights to scan direcotry at \"{path}\".");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Unexpected error occured while reading directories at: \"{path}\"");
            }

            return directories;
        }

        public static DirectoryWithCsv ScanDirectory(string rootPath, string path)
        {
            List<string> csvFiles = null;
            try
            {
                csvFiles = new List<string>(
                       Directory.GetFiles(path, "*.csv", SearchOption.TopDirectoryOnly));
            }
            catch (UnauthorizedAccessException)
            {
                Console.WriteLine($"Insufficient rights to read all files in \"{path}\".");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Unexpected error occured while scanning directories at: \"{path}\"");
            }

            var directoryPath = Regex.Replace(path, Regex.Escape(rootPath), ".");
            directoryPath = directoryPath == "." ? Constants.ROOT_DIRECTORY : directoryPath;

            return (csvFiles != null && csvFiles.Count > 0)
                ? new DirectoryWithCsv(directoryPath, csvFiles)
                : null;
        }

        public static List<CsvFileConfiguration> LoadFileConfigurationsFile(string configurationsFilePath)
        {
            try
            {
                return JsonServices.DeserializeJson<List<CsvFileConfiguration>>(configurationsFilePath, "Csv file configurations");
            }
            catch (Exception)
            {
                Console.WriteLine($"Creating new {Path.GetFileName(configurationsFilePath)} in {configurationsFilePath}");

                File.Create(configurationsFilePath);

                return null;
            }
        }

        public static BitmapImage SetBitmapImageFromPath(string path)
        {
            BitmapImage newImage;

            if (File.Exists(path))
            {
                newImage = new BitmapImage(new Uri(path));
            }
            else
            {
                newImage = ResourceHelper.LoadBitmapFromResource("images/no_image_available.png");
            }

            return newImage;
        }

        public static bool IsImageFile(string fileAbsPath, string[] fileExtensions = null)
        {
            fileExtensions ??= BASE_IMAGE_FILE_EXTENSIONS;

            try
            {
                return fileExtensions.Contains(Path.GetExtension(fileAbsPath));
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error reading file extension for file: {fileAbsPath}");
            }

            return false;
        }
    }
}
