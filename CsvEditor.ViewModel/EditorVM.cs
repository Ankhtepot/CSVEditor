using CSVEditor.Model;
using CSVEditor.Model.Services;
using CSVEditor.ViewModel.BackgroundWorkers;
using JetBrains.Annotations;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using static CSVEditor.Model.Enums;

namespace CSVEditor.ViewModel
{
    public class EditorVM : INotifyPropertyChanged
    {
        public const string APP_OPTIONS_FILE_NAME = "options.json";
        public const string CSV_CONFIGURATIONS_FILE_NAME = "csv_conf.json";
        public const string CONFIGURATION_FOLDER_NAME = "config";

        public readonly DirectoryWithCsv DEFAULT_DIRECTORY = new DirectoryWithCsv("Directory", new List<string> { "Files..." });

        private string rootRepositoryPath;

        public static AppOptions AppOptions;

        public static string BaseAppPath;
        public static string ConfigurationFolderPath;

        public string RootRepositoryPath
        {
            get => rootRepositoryPath;
            set
            {
                rootRepositoryPath = value;
                if (value != null && AppOptions != null) AppOptions.LastRootPath = value;
                OnPropertyChanged();
            }
        }

        private bool isGitRepo;

        public bool IsGitRepo
        {
            get { return isGitRepo; }
            set { isGitRepo = value; OnPropertyChanged(); }
        }

        private bool isLineEditMode;
        public bool IsLineEditMode
        {
            get { return isLineEditMode; }
            set { isLineEditMode = value; OnPropertyChanged(); }
        }

        private string selectedText;
        public string SelectedText
        {
            get { return selectedText; }
            set
            {
                selectedText = value; OnPropertyChanged();
            }
        }

        //TODO: remove if not neccesarry
        private DirectoryWithCsv selectedDirectory;
        public DirectoryWithCsv SelectedDirectory
        {
            get { return selectedDirectory; }
            set
            {
                selectedDirectory = value;
                Console.WriteLine("Selected Directory: " + selectedDirectory?.DirectoryAbsolutePath);
                OnPropertyChanged();
            }
        }

        private string selectedFile;
        public string SelectedFile
        {
            get
            {
                return string.IsNullOrEmpty(selectedFile) ? Constants.NO_FILE_SELECTED : selectedFile;
            }
            set
            {
                SetSelectedFile(value);
                OnPropertyChanged();
            }
        }

        private CsvFile selectedCsvFile;
        public CsvFile SelectedCsvFile
        {
            get { return selectedCsvFile; }
            set
            {
                SetSelectedCsvFile(value);
                OnPropertyChanged();
            }
        }

        private int selectedItemIndex;

        public int SelectedItemIndex
        {
            get { return selectedItemIndex; }
            set
            {
                selectedItemIndex = value;
                OnPropertyChanged();
            }
        }

        private AsyncVM asyncVM;
        public AsyncVM AsyncVM
        {
            get { return asyncVM; }
            set { asyncVM = value; OnPropertyChanged(); }
        }

        public ObservableCollection<CsvFile> CsvFiles { get; set; }

        public ObservableCollection<DirectoryWithCsv> CsvFilesStructure { get; set; }

        private ObservableCollection<CsvFileConfiguration> FileConfigurations { get; set; } //TODO List or ObsCollection? Manage update on change in ConfigEditor

        //*******************************
        //********** Commands ***********
        //*******************************

        public DelegateCommand LoadRepositoryCommand { get; set; }
        public DelegateCommand CancelActiveWorkerAsyncCommand { get; set; }
        public DelegateCommand SwitchEditModeCommand { get; set; }

        //*******************************
        //********* Constructor *********
        //*******************************

        public EditorVM()
        {
            BaseAppPath = Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);
            ConfigurationFolderPath = Path.Combine(BaseAppPath, CONFIGURATION_FOLDER_NAME);
            AsyncVM = new AsyncVM(this);

            RootRepositoryPath = Constants.LOAD_REPOSITORY_PLACEHOLDER;
            IsLineEditMode = false;
            IsGitRepo = false;
            SelectedText = Constants.SELECTED_TEXT_DEFAULT;
            AsyncVM.WorkingStatus = WorkStatus.Idle;
            CsvFilesStructure = new ObservableCollection<DirectoryWithCsv>();
            CsvFilesStructure.Add(DEFAULT_DIRECTORY);

            LoadRepositoryCommand = new DelegateCommand(AsyncVM.LoadRepository, AsyncVM.LoadRepository_CanExecute);
            CancelActiveWorkerAsyncCommand = new DelegateCommand(AsyncVM.CancelActiveWorkerAsync);
            SwitchEditModeCommand = new DelegateCommand(SwitchLineEditMode);

            setAppOptions();
        }

        //*******************************
        //*********** Methods ***********
        //*******************************

        public void SetSelectedFile(string value, bool needsProcessing = true)
        {
            selectedFile = value;

            if (File.Exists(value))
            {
                Console.WriteLine("MainVM.SelectedCsvFile: " + selectedFile);
                if (needsProcessing)
                {
                    new GetCsvFileFromPathWorker(this).RunAsync(SelectedFile);
                }

                AsyncVM.SetRawTextFromAbsPath(SelectedFile);

                AppOptions.LastSelectedFilePath = SelectedFile;
            }
            else
            {
                SelectedCsvFile = new CsvFile();
                AsyncVM.SelectedFileRaw = Constants.NO_FILE_SELECTED;
                AppOptions.LastSelectedFilePath = null;
            }
        }

        private void SetSelectedCsvFile(CsvFile value)
        {
            if (AppOptions != null) AppOptions.LastSelectedCsvFile = value;
            value.columnConfigurations = ResolveCsvFileConfiguration(
                value.columnConfigurations,
                value.AbsPath,
                Path.Combine(ConfigurationFolderPath, CSV_CONFIGURATIONS_FILE_NAME));
            selectedCsvFile = value;
        }

        private List<CsvColumnConfiguration> ResolveCsvFileConfiguration(
            List<CsvColumnConfiguration> currentFileConfigurations,
            string currentFileAbsPath,
            string configurationsFilePath)
        {
            ResolveFileConfigurationsFile(currentFileConfigurations, currentFileAbsPath, configurationsFilePath);

            return GetConfigurationForCurrentFile(currentFileConfigurations, currentFileAbsPath);
        }

        private void ResolveFileConfigurationsFile(List<CsvColumnConfiguration> currentFileConfigurations, string currentFileAbsPath, string configurationsFilePath)
        {
            if (FileConfigurations == null) // if configuration file wasn't loaded yet
            {
                FileConfigurations = new ObservableCollection<CsvFileConfiguration>();

                var configurations = JsonServices.DeserializeJson<List<CsvFileConfiguration>>(configurationsFilePath, "Csv file configurations");

                if (configurations == null) // if configuration file dosn't exist
                {
                    Console.WriteLine($"Creating new {CSV_CONFIGURATIONS_FILE_NAME}");

                    var newFileConfiguration = new CsvFileConfiguration()
                    {
                        AbsoluteFilePath = currentFileAbsPath,
                        ColumnConfigurations = currentFileConfigurations
                    };

                    var newFileConfigurations = new List<CsvFileConfiguration>() { newFileConfiguration };

                    JsonServices.SerializeJson(
                        newFileConfigurations,
                        Path.Combine(ConfigurationFolderPath, CSV_CONFIGURATIONS_FILE_NAME),
                        "Csv files configurations");

                    FileConfigurations.Add(newFileConfiguration);
                }
                else // if configuration file already exists
                {
                    FileConfigurations.Clear();
                    configurations.ForEach(configuration => FileConfigurations.Add(configuration));
                }
            }
        }

        private List<CsvColumnConfiguration> GetConfigurationForCurrentFile(List<CsvColumnConfiguration> currentFileConfigurations, string currentFileAbsPath)
        {
            var foundConfiguration = FileConfigurations
                .Where(conf => conf.AbsoluteFilePath == currentFileAbsPath)
                .FirstOrDefault().ColumnConfigurations;

            return foundConfiguration != null
                ? foundConfiguration
                : currentFileConfigurations;
        }

        private void setAppOptions()
        {
            AppOptions = new AppOptions();

            CheckConfigDirectory();

            var loadedOptions = JsonServices.DeserializeJson<AppOptions>(
                Path.Combine(ConfigurationFolderPath, APP_OPTIONS_FILE_NAME),
                "App Options");

            if (loadedOptions != null)
            {
                RootRepositoryPath = loadedOptions.LastRootPath;
                SetSelectedFile(loadedOptions.LastSelectedFilePath, false);
                SelectedCsvFile = loadedOptions.LastSelectedCsvFile;
                IsGitRepo = FileSystemServices.IsDirectoryWithGitRepository(RootRepositoryPath);
                CsvFilesStructure.Clear();
                loadedOptions.LastCsvFilesStructure.ForEach(record => CsvFilesStructure.Add(record));
                AppOptions.LastCsvFilesStructure = CsvFilesStructure.ToList(); // Setting whole new CsvFileStructure instead of line by line via CsvFilesStructure OnChange event
                SelectedItemIndex = 0;
            }
        }

        private void CheckConfigDirectory()
        {
            if (!Directory.Exists(Path.Combine(BaseAppPath, CONFIGURATION_FOLDER_NAME)))
            {
                Console.WriteLine($"Creating new {CONFIGURATION_FOLDER_NAME} directory in {BaseAppPath}");
                Directory.CreateDirectory(Path.Combine(BaseAppPath, CONFIGURATION_FOLDER_NAME));
            }
        }

        private void SwitchLineEditMode()
        {
            SetLineEditMode(!IsLineEditMode);
        }

        private void SetLineEditMode(bool lineEditMode)
        {
            IsLineEditMode = lineEditMode;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
