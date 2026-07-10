using CSVEditor.Core.HelperClasses;
using CSVEditor.Core.Interfaces;
using CSVEditor.Core.Services;
using CSVEditor.Core.Properties;
using Prism.Commands;
using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;

namespace CSVEditor.ViewModel
{
    public class SaveVM : INotifyPropertyChanged
    {
        public string CsvFileText;
        public Window SaveWindow;
        public bool SaveSuccessful;
        public Visibility IsLoggedInToGit { get; set; } = Visibility.Collapsed;
        public Visibility IsNotLoggedInToGit => IsLoggedInToGit == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;

        public string CsvFilePath
        {
            get;
            set
            {
                field = value;
                OnPropertyChanged();
            }
        }

        public SaveOptions SaveOptions
        {
            get;
            set
            {
                field = value;
                OnPropertyChanged();
            }
        }

        public DelegateCommand OverwriteCommand { get; set; }
        public DelegateCommand SaveAsCommand { get; set; }
        public DelegateCommand SaveAlternativePathCommand { get; set; }
        public DelegateCommand CancelCommand { get; set; }

        public static Action<bool, bool> OnSaved;

        public SaveVM()
        {
            OverwriteCommand = new DelegateCommand(Overwrite);
            SaveAsCommand = new DelegateCommand(SaveAs);
            SaveAlternativePathCommand = new DelegateCommand(SaveAlternativePath);
            CancelCommand = new DelegateCommand(Cancel);
            
            IsLoggedInToGit = AppOptionsService.IsLoggedInToGit == true ? Visibility.Visible : Visibility.Collapsed;
        }

        public static async void SaveCurrentCsvFile(EditorVM context, IWindowService windowService)
        {
            SaveOptions saveOptions = EditorVM.AppOptions.SaveOptions.RememberSetting
                ? EditorVM.AppOptions.SaveOptions
                : new SaveOptions();

            string csvText = await context.AsyncVM.CsvFileToTextTask(context.SelectedCsvFile);

            SaveOptions saveWindowResult = windowService.OpenSaveWindow(saveOptions, csvText, context.SelectedCsvFile.AbsPath);
            if (saveWindowResult != null)
            {
                EditorVM.AppOptions.SaveOptions = saveWindowResult;
                OnSaved?.Invoke(saveWindowResult.CommitOnSave, saveWindowResult.PushOnSave);
                context.IsFileEdited = false;
            }

            if (context.ShouldExitAfterSave)
            {
                Application.Current.Shutdown();
            }
        }

        public static void SaveConfiguration()
        {
            JsonServices.SerializeJson(EditorVM.FileConfigurations,
                Path.Combine(EditorVM.ConfigurationFolderPath, Constants.CSV_CONFIGURATIONS_FILE_NAME),
                Resources.CsvFileConfigurationsText);
        }

        private void SaveAlternativePath()
        {
            string path = GetAlternativePathIsExists();

            SaveOptions.AlternativePath = path;

            SaveFile(path);
        }

        private void SaveAs()
        {
            string path = GetAlternativePathIsExists();

            path = FileSystemService.QueryUserToSaveFile(path, Constants.SAVE_FILE_TITLE);

            SaveOptions.AlternativePath = path;

            SaveFile(path);
        }

        private string GetAlternativePathIsExists()
        {
            return string.IsNullOrEmpty(SaveOptions.AlternativePath)
                ? Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
                : SaveOptions.AlternativePath;
        }

        private void Overwrite()
        {
            SaveFile(CsvFilePath);
            Cancel();
        }

        private void SaveFile(string path)
        {
            SaveSuccessful = FileSystemService.SaveTextFile(path, CsvFileText);

            Cancel();
        }

        private void Cancel()
        {
            SaveWindow?.Close();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
