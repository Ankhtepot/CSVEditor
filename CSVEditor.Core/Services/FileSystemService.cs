using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media.Imaging;
using CSVEditor.Core.HelperClasses;
using CSVEditor.Core.Properties;
using Microsoft.Win32;
using static CSVEditor.Core.HelperClasses.Enums;

namespace CSVEditor.Core.Services
{
    public static class FileSystemService
    {
        private static string _baseAppPath;

        public static string BaseAppPath => _baseAppPath
            ??= Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

        private static string _configurationFolderPath;
        private static AppOptions AppOptions => AppOptionsService.AppOptions;

        public static string ConfigurationFolderPath => _configurationFolderPath
            ??= Path.Combine(BaseAppPath ?? string.Empty, Constants.CONFIGURATION_FOLDER_NAME);

        private static readonly string[] BaseImageFileExtensions = [".png", ".jpg", ".jpeg", ".webp"];
        private const string AllFilesFilter = "All files (*.*)|*.*";

        public static string QueryUserForRootRepositoryPath(string title = "")
        {
            OpenFolderDialog dialog = new();

            if (!string.IsNullOrEmpty(title))
            {
                dialog.Title = title;
            }

            return dialog.ShowDialog() == true
                ? dialog.FolderName
                : Constants.LOAD_REPOSITORY_FAILED;
        }

        public static string QueryUserForPath(
            string initialDirectory = "",
            string title = "",
            string filter = null,
            DialogPathType dialogPathType = DialogPathType.None
        )
        {
            if (!string.IsNullOrEmpty(filter))
            {
                OpenFileDialog fileDialog = new()
                {
                    Filter = filter,
                    CheckFileExists = false,
                    FileName = Resources.FolderSelectionText,
                    Title = title,
                    InitialDirectory = ResolveInitialDirectory(initialDirectory, dialogPathType)
                };

                return fileDialog.ShowDialog() == true
                    ? Path.GetDirectoryName(fileDialog.FileName)
                    : null;
            }

            OpenFolderDialog dialog = new();

            if (!string.IsNullOrEmpty(title))
            {
                dialog.Title = title;
            }

            dialog.InitialDirectory = ResolveInitialDirectory(initialDirectory, dialogPathType);

            try
            {
                if (dialog.ShowDialog() == true)
                {
                    StoreUsedPath(dialog.FolderName, dialogPathType);
                    return dialog.FolderName;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(Resources.ErrorReadingDirectoryPathFormat, e.Message);
            }

            return null;
        }

        public static string QueryUserToSelectImageFile(string path, string title = "", string filter = null)
        {
            filter ??= AllFilesFilter;

            string fileName = File.Exists(path) ? Path.GetFileName(path) : null;
            string cleanedPath = string.IsNullOrEmpty(fileName) ? path : Path.GetDirectoryName(path);

            OpenFileDialog fileDialog = new()
            {
                Filter = filter,
                CheckPathExists = true,
                InitialDirectory = ResolveInitialDirectory(cleanedPath, DialogPathType.Open),
                FileName = string.IsNullOrEmpty(fileName) ? Resources.NewFileText : fileName,
            };

            if (!string.IsNullOrEmpty(title))
            {
                fileDialog.Title = title;
            }

            if (fileDialog.ShowDialog() != true) 
                return null;
            
            StoreUsedPath(Path.GetDirectoryName(fileDialog.FileName), DialogPathType.Open);
            return fileDialog.FileName;
        }

        public static string QueryUserToSaveFile(string path, string title = "", string filter = null)
        {
            filter ??= AllFilesFilter;

            SaveFileDialog fileDialog = new()
            {
                Filter = filter,
                CheckPathExists = true,
                InitialDirectory = ResolveInitialDirectory(Path.GetDirectoryName(path), DialogPathType.Save),
            };

            if (!string.IsNullOrEmpty(title))
            {
                fileDialog.Title = title;
            }

            try
            {
                fileDialog.ShowDialog();

                if (!string.IsNullOrEmpty(fileDialog.FileName))
                {
                    StoreUsedPath(Path.GetDirectoryName(fileDialog.FileName), DialogPathType.Save);
                    return fileDialog.FileName;
                }
            }
            catch (Exception e)
            {
                MessageBoxHelper.ShowProcessErrorBox(title, Constants.SAVING_FAILED_TEXT + e.Message);
            }
            
            return null;
        }

        public static bool IsDirectoryWithGitRepository(string rootPath)
        {
            List<string> rootPathDirectories = new();

            try
            {
                rootPathDirectories = Directory.GetDirectories(rootPath, @".git").ToList();
            }
            catch (Exception e)
            {
                Console.WriteLine(Resources.ErrorCheckingGitRepoFormat, e.Message);
            }

            if (rootPathDirectories.Count > 0)
            {
                foreach (string directory in rootPathDirectories)
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

            IEnumerable<string> directories = GetDirectoriesFromPath(rootPath, worker) ?? new List<string>();

            foreach (string directory in directories.Where(dir => !Regex.Match(dir, "\\.git").Success))
            {
                List<string> recursiveYield = GetDirectoriesFromRootPath(directory, worker);
                directories = recursiveYield != null
                    ? directories?.Concat(recursiveYield).ToList()
                    : null;
            }

            return directories?.ToList();
        }
        
        private static void StoreUsedPath(string dialogFolderName, DialogPathType dialogPathType)
        {
            if (string.IsNullOrEmpty(dialogFolderName))
                return;
            
            switch (dialogPathType)
            {
                case DialogPathType.Save:
                    AppOptions.LastSavedDirectoryPath = dialogFolderName;
                    break;
                case DialogPathType.Open:
                    AppOptions.LastOpenedDirectoryPath = dialogFolderName;
                    break;
                case DialogPathType.None:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(dialogPathType), dialogPathType, null);
            }
        }

        private static string ResolveInitialDirectory(string initialDirectory,
            DialogPathType dialogPathType = DialogPathType.None)
        {
            if (!string.IsNullOrEmpty(initialDirectory))
                return Directory.Exists(initialDirectory)
                    ? initialDirectory
                    : Path.GetDirectoryName(initialDirectory) ?? "";

            return dialogPathType switch
            {
                DialogPathType.Save => AppOptions.LastSavedDirectoryPath,
                DialogPathType.Open => AppOptions.LastOpenedDirectoryPath,
                _ => Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
            };
        }

        private static List<string> GetDirectoriesFromPath(string path, BackgroundWorker worker = null)
        {
            if (worker is {CancellationPending: true})
            {
                return null;
            }

            List<string> directories = new();

            try
            {
                directories = Directory.GetDirectories(path).ToList();
            }
            catch (UnauthorizedAccessException e)
            {
                Console.WriteLine(Resources.InsufficientRightsToScanDirectoryFormat, path, e.Message);
            }
            catch (Exception e)
            {
                Console.WriteLine(Resources.UnexpectedErrorReadingDirectoriesFormat, path, e.Message);
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
                Console.WriteLine(Resources.InsufficientRightsToReadAllFilesFormat, path);
            }
            catch (Exception e)
            {
                Console.WriteLine(Resources.UnexpectedErrorScanningDirectoriesFormat, path, e.Message);
            }

            string directoryPath = Regex.Replace(path, Regex.Escape(rootPath), ".");
            directoryPath = directoryPath == "." ? Constants.ROOT_DIRECTORY : directoryPath;

            return csvFiles is {Count: > 0}
                ? new DirectoryWithCsv(directoryPath, csvFiles)
                : null;
        }

        public static List<CsvFileConfiguration> LoadFileConfigurationsFile(string configurationsFilePath)
        {
            try
            {
                return JsonServices.DeserializeJson<List<CsvFileConfiguration>>(configurationsFilePath,
                    Resources.CsvFileConfigurationsText);
            }
            catch (Exception)
            {
                Console.WriteLine(Resources.CreatingNewFileInDirectoryFormat, Path.GetFileName(configurationsFilePath),
                    configurationsFilePath);

                File.Create(configurationsFilePath);

                return null;
            }
        }

        public static BitmapImage GetBitmapImageFromPath(string path)
        {
            BitmapImage newImage = new();
            newImage.BeginInit();
            newImage.CacheOption = BitmapCacheOption.OnLoad;

            newImage.UriSource = File.Exists(path)
                ? new Uri(path)
                : ResourceHelper.LoadBitmapUriSourceFromResource(Constants.IMAGE_NOT_AVAILABLE_APP_PATH);

            try
            {
                newImage.EndInit();
            }
            catch (Exception e)
            {
                Console.WriteLine(Resources.CouldNotFindImagePathError, e);
                return null;
            }

            return newImage;
        }

        public static bool IsImageFile(string fileAbsPath, string[] fileExtensions = null)
        {
            fileExtensions ??= BaseImageFileExtensions;

            try
            {
                return fileExtensions.Contains(Path.GetExtension(fileAbsPath).ToLower());
            }
            catch (Exception e)
            {
                Console.WriteLine(Resources.ErrorReadingFileExtensionFormat, fileAbsPath, e.Message);
            }

            return false;
        }

        public static bool SaveImageFile(string newImageFile, string selectedSavePath)
        {
            string newFileName = Path.GetFileName(newImageFile);

            string title = Resources.ConfirmSavingImageFileTitle;
            string message = string.Format(Resources.ConfirmSavingImageFileMessage, newFileName, selectedSavePath);
            string messageOverwrite =
                string.Format(Resources.FileAlreadyExistsOverwriteMessage, newFileName, selectedSavePath);
            MessageBoxImage icon = MessageBoxImage.Question;
            MessageBoxButton buttons = MessageBoxButton.OKCancel;
            string fullNewFilePath = Path.Combine(selectedSavePath, newFileName);

            if (File.Exists(fullNewFilePath))
            {
                if (MessageBox.Show(messageOverwrite, title, MessageBoxButton.YesNo, MessageBoxImage.Warning) ==
                    MessageBoxResult.OK)
                {
                    File.Copy(newImageFile, fullNewFilePath, true);
                }

                return true;
            }

            if (MessageBox.Show(message, title, buttons, icon) != MessageBoxResult.OK)
                return false;

            File.Copy(newImageFile, fullNewFilePath);
            return true;
        }

        public static string ConvertContentPathToSystemPath(string imageCellContent)
        {
            string path = "";

            switch (string.IsNullOrEmpty(imageCellContent))
            {
                case false:
                {
                    path = Regex.Replace(imageCellContent, "\r", "");
                    path = Regex.Replace(path, "/", @"\");

                    if (!char.IsLetterOrDigit(path[0]))
                    {
                        path = path[1..];
                    }

                    break;
                }
            }

            return path;
        }

        public static void ValidateConfigDirectory(string baseAppPath, string configurationFolderName)
        {
            if (!Directory.Exists(ConfigurationFolderPath))
            {
                Console.WriteLine(Resources.CreatingNewDirectoryInDirectoryFormat, configurationFolderName,
                    baseAppPath);
                Directory.CreateDirectory(ConfigurationFolderPath);
            }
        }

        public static bool SaveTextFile(string fileName, string fileContent)
        {
            try
            {
                if (!Directory.Exists(Path.GetDirectoryName(fileName)))
                {
                    Directory.CreateDirectory(fileName);
                }

                File.WriteAllText(fileName, fileContent);
                return true;
            }
            catch (Exception e)
            {
                MessageBoxHelper.ShowProcessErrorBox(Constants.SAVE_FILE_TITLE,
                    Constants.SAVING_FAILED_TEXT + e.Message);
                return false;
            }
        }
    }
}