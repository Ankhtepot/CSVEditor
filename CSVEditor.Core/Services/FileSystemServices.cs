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
namespace CSVEditor.Core.Services
{
    public class FileSystemServices
    {
        private static readonly string[] BaseImageFileExtensions = { ".png", ".jpg", ".jpeg", ".webp" };
        private static readonly string AllFilesFilter = "All files (*.*)|*.*";

        public static string QueryUserForRootRepositoryPath(string title = "")
        {
            var dialog = new OpenFolderDialog();
            
            if (!string.IsNullOrEmpty(title))
            {
                dialog.Title = title;
            }

            if (dialog.ShowDialog() == true)
            {
                return dialog.FolderName;
            }
            else
            {
                return Constants.LOAD_REPOSITORY_FAILED;
            }
        }

        public static string QueryUserForPath(string initialDirectory = "", string title = "", string filter = null)
        {
            if (!string.IsNullOrEmpty(filter))
            {
                var fileDialog = new OpenFileDialog
                {
                    Filter = filter,
                    CheckFileExists = false,
                    FileName = Resources.FolderSelectionText,
                    Title = title,
                    InitialDirectory = Directory.Exists(initialDirectory)
                        ? initialDirectory
                        : (string.IsNullOrEmpty(initialDirectory) ? "" : Path.GetDirectoryName(initialDirectory))
                };

                if (fileDialog.ShowDialog() == true)
                {
                    return Path.GetDirectoryName(fileDialog.FileName);
                }

                return null;
            }

            var dialog = new OpenFolderDialog();

            if (!string.IsNullOrEmpty(title))
            {
                dialog.Title = title;
            }

            if (!string.IsNullOrEmpty(initialDirectory) && Directory.Exists(initialDirectory))
            {
                dialog.InitialDirectory = initialDirectory;
            }

            try
            {
                if (dialog.ShowDialog() == true)
                {
                    return dialog.FolderName;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(string.Format(Resources.ErrorReadingDirectoryPathFormat, e.Message));
            }

            return null;
        }

        public static string QueryUserToSelectFile(string path, string title = "", string filter = null)
        {
            filter ??= AllFilesFilter;

            var fileName = Path.GetFileName(path);
            var cleanedPath = Path.GetDirectoryName(path);

            var fileDialog = new OpenFileDialog()
            {
                Filter = filter,
                CheckPathExists = true,
                InitialDirectory = (Directory.Exists(cleanedPath) 
                    ? cleanedPath 
                    : Environment.GetFolderPath(Environment.SpecialFolder.CommonPictures)) 
                                   ?? Environment.SpecialFolder.CommonDocuments.ToString(),
                FileName = string.IsNullOrEmpty(fileName) ? Resources.NewFileText : fileName,
            };

            if (!string.IsNullOrEmpty(title))
            {
                fileDialog.Title = title;
            }

            if (fileDialog.ShowDialog() == true)
            {
                return fileDialog.FileName;
            }

            return null;
        }

        public static string QueryUserToSaveFile(string path, string title = "", string filter = null)
        {
            filter ??= AllFilesFilter;

            var fileDialog = new SaveFileDialog()
            {
                Filter = filter,
                CheckPathExists = true,
                InitialDirectory = Directory.Exists(path) ? path : Environment.GetFolderPath(Environment.SpecialFolder.CommonPictures),
            };

            if (!string.IsNullOrEmpty(title))
            {
                fileDialog.Title = title;
            }

            try
            {
                fileDialog.ShowDialog();
                return fileDialog.FileName;
            }
            catch (Exception e)
            {
                MessageBoxHelper.ShowProcessErrorBox(title, Constants.SAVING_FAILED_TEXT + e.Message);
                return null;
            }
        }

        public static bool IsDirectoryWithGitRepository(string rootPath)
        {
            var rootPathDirectories = new List<string>();

            try
            {
                rootPathDirectories = Directory.GetDirectories(rootPath, @".git").ToList();
            }
            catch (Exception e)
            {
                Console.WriteLine(string.Format(Resources.ErrorCheckingGitRepoFormat, e.Message));
            }

            if (rootPathDirectories.Count > 0)
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

            IEnumerable<string> directories = GetDirectoriesFromPath(rootPath, worker) ?? new List<string>();

            foreach (var directory in directories.Where(dir => !Regex.Match(dir, "\\.git").Success))
            {
                var recursiveYield = GetDirectoriesFromRootPath(directory, worker);
                directories = recursiveYield != null
                    ? directories?.Concat(recursiveYield).ToList()
                    : null;
            }

            return directories?.ToList();
        }

        private static List<string> GetDirectoriesFromPath(string path, BackgroundWorker worker = null)
        {
            if (worker != null && worker.CancellationPending)
            {
                return null;
            }

            var directories = new List<string>();

            try
            {
                directories = Directory.GetDirectories(path).ToList();
            }
            catch (UnauthorizedAccessException e)
            {
                Console.WriteLine(string.Format(Resources.InsufficientRightsToScanDirectoryFormat, path, e.Message));
            }
            catch (Exception e)
            {
                Console.WriteLine(string.Format(Resources.UnexpectedErrorReadingDirectoriesFormat, path, e.Message));
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
                Console.WriteLine(string.Format(Resources.InsufficientRightsToReadAllFilesFormat, path));
            }
            catch (Exception e)
            {
                Console.WriteLine(string.Format(Resources.UnexpectedErrorScanningDirectoriesFormat, path, e.Message));
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
                return JsonServices.DeserializeJson<List<CsvFileConfiguration>>(configurationsFilePath, Resources.CsvFileConfigurationsText);
            }
            catch (Exception)
            {
                Console.WriteLine(string.Format(Resources.CreatingNewFileInDirectoryFormat, Path.GetFileName(configurationsFilePath), configurationsFilePath));

                File.Create(configurationsFilePath);

                return null;
            }
        }

        public static BitmapImage GetBitmapImageFromPath(string path)
        {
            var newImage = new BitmapImage();
            newImage.BeginInit();
            newImage.CacheOption = BitmapCacheOption.OnLoad;

            newImage.UriSource = File.Exists(path) 
                ? new Uri(path) 
                : ResourceHelper.LoadBitmapUriSourceFromResource(Constants.IMAGE_NOT_AVAILABLE_APP_PATH);

            newImage.EndInit();

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
                Console.WriteLine(string.Format(Resources.ErrorReadingFileExtensionFormat, fileAbsPath, e.Message));
            }

            return false;
        }

        public static bool SaveImageFile(string newImageFile, string selectedSavePath)
        {
            var newFileName = Path.GetFileName(newImageFile);

            var title = Resources.ConfirmSavingImageFileTitle;
            var message = string.Format(Resources.ConfirmSavingImageFileMessage, newFileName, selectedSavePath);
            var messageOverwrite = string.Format(Resources.FileAlreadyExistsOverwriteMessage, newFileName, selectedSavePath);
            var icon = MessageBoxImage.Question;
            var buttons = MessageBoxButton.OKCancel;
            var fullNewFilePath = Path.Combine(selectedSavePath, newFileName);

            if (File.Exists(fullNewFilePath))
            {
                if (MessageBox.Show(messageOverwrite, title, MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.OK)
                {
                    File.Copy(newImageFile, fullNewFilePath, true);
                    return true;
                }

                return true;
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
                Console.WriteLine(string.Format(Resources.CreatingNewDirectoryInDirectoryFormat, configurationFolderName, baseAppPath));
                Directory.CreateDirectory(Path.Combine(baseAppPath, configurationFolderName));
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
                MessageBoxHelper.ShowProcessErrorBox(Constants.SAVE_FILE_TITLE, Constants.SAVING_FAILED_TEXT + e.Message);
                return false;
            }
        }
    }
}
