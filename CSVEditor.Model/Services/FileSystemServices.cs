using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media.Imaging;
using CSVEditor.Model;
using CSVEditor.Model.HelperClasses;
using CSVEditor.Model.Services;
using Microsoft.Win32;
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

        public static string QueryUserToSelectFile(string path, string title = "", string filter = "All files (*.*)|*.*")
        {
            var fileDialog = new OpenFileDialog()
            {
                Filter = filter,
                CheckPathExists = true,
                InitialDirectory = Directory.Exists(path) ? path : Environment.GetFolderPath(Environment.SpecialFolder.CommonPictures)
            };

            if (fileDialog.ShowDialog() == true)
            {
                return fileDialog.FileName;
            }

            return null;
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
            BitmapImage newImage = new BitmapImage();
            newImage.BeginInit();
            newImage.CacheOption = BitmapCacheOption.OnLoad;

            if (File.Exists(path))
            {
                newImage.UriSource = new Uri(path);
            }
            else
            {
                newImage.UriSource = ResourceHelper.LoadBitmapUriSourceFromResource("images/no_image_available.png");
            }

            newImage.EndInit();

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

        public static bool SaveDraggedFile(string newImageFile, string selectedSavePath)
        {
            var newFileName = Path.GetFileName(newImageFile);

            var title = "Confirm Saving Image File";
            var message = $"Are you sure you want to save file {newFileName} to:\n{selectedSavePath} ?";
            var messageOverwrite = $"File {newFileName} already exists at :\n{selectedSavePath} .\nDo you want to overwrite this file?";
            var icon = MessageBoxImage.Question;
            var buttons = MessageBoxButton.OKCancel;
            var fullNewFilePath = Path.Combine(selectedSavePath, newFileName);

            if (File.Exists(fullNewFilePath))
            {
                //TODO: if file already exists, pop up windows with preview and ask if user wants to replace that file.
                if (MessageBox.Show(messageOverwrite, title, buttons, MessageBoxImage.Warning) == MessageBoxResult.OK)
                {
                    File.Copy(newImageFile, fullNewFilePath, true);
                    return true;
                }

                return false;
            }

            if (MessageBox.Show(message, title, buttons, icon) == MessageBoxResult.OK)
            {
                File.Copy(newImageFile, fullNewFilePath);
                return true;
            }

            return false;
        }

        public static string ConvertContentPathToSystemPath(string imageCellContent)
        {
            var path = "";

            if (!string.IsNullOrEmpty(imageCellContent))
            {
                path = Regex.Replace(imageCellContent, "\r", "");
                path = Regex.Replace(path, "/", @"\");

                if (!char.IsLetterOrDigit(path[0]))
                {
                    path = path.Substring(1);
                }
            }

            return path;
        }

        public static void ValidateConfigDirectory(string baseAppPath, string configurationFolderName)
        {
            if (!Directory.Exists(Path.Combine(baseAppPath, configurationFolderName)))
            {
                Console.WriteLine($"Creating new {configurationFolderName} directory in {baseAppPath}");
                Directory.CreateDirectory(Path.Combine(baseAppPath, configurationFolderName));
            }
        }

        public static void SaveTextFile(string fileName, string fileContent)
        {
            try
            {
                if (!Directory.Exists(Path.GetDirectoryName(fileName)))
                {
                    Directory.CreateDirectory(fileName);
                }

                File.WriteAllText(fileName, fileContent);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error writing to path \"{fileName}\". Error: {e.Message}.");
            }
        }
    }
}
