using CSVEditor.Model.HelperClasses;
using CSVEditor.Model.Interfaces;
using CSVEditor.Model.Services;
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

        private string csvFilePath;
        public string CsvFilePath
        {
            get { return csvFilePath; }
            set
            {
                csvFilePath = value;
                OnPropertyChanged();
            }
        }

        private SaveOptions saveOptions;
        public SaveOptions SaveOptions
        {
            get { return saveOptions; }
            set
            {
                saveOptions = value;
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
        }

        public static async void SaveCurrentCsvFile(EditorVM context, IWindowService windowService)
        {
            var saveOptions = EditorVM.AppOptions.SaveOptions.RememberSetting
                ? EditorVM.AppOptions.SaveOptions
                : new SaveOptions();

            var csvText = await context.AsyncVM.CsvFileToTextTask(context.SelectedCsvFile);

            var saveWindowResult = windowService.OpenSaveWindow(saveOptions, csvText, context.SelectedCsvFile.AbsPath);
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
                Path.Combine(EditorVM.ConfigurationFolderPath, EditorVM.CSV_CONFIGURATIONS_FILE_NAME),
                "Csv file configurations");
        }

        public static void SaveAppOptions()
        {
            JsonServices.SerializeJson(EditorVM.AppOptions,
                Path.Combine(EditorVM.ConfigurationFolderPath, EditorVM.APP_OPTIONS_FILE_NAME),
                "App Options");
        }

        private void SaveAlternativePath()
        {
            var path = GetAlternativePathIsExists();

            SaveOptions.AlternativePath = path;

            SaveFile(path);
        }

        private void SaveAs()
        {
            var path = GetAlternativePathIsExists();

            path = FileSystemServices.QueryUserToSaveFile(path, Constants.SAVE_FILE_TITLE);

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
            SaveSuccessful = FileSystemServices.SaveTextFile(path, CsvFileText);

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
