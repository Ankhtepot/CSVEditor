using CSVEditor.Model;
using CSVEditor.Model.HelperClasses;
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
using System.Windows;
using System.Windows.Controls;
using static CSVEditor.Model.HelperClasses.Enums;

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

        private string selectedFile;
        public string SelectedFile
        {
            get
            {
                return string.IsNullOrEmpty(selectedFile) ? Constants.NO_FILE_SELECTED : selectedFile;
            }
            set
            {
                setSelectedFile(value);
                OnPropertyChanged();
            }
        }

        private CsvFile selectedCsvFile;
        public CsvFile SelectedCsvFile
        {
            get { return selectedCsvFile; }
            set
            {
                setSelectedCsvFile(value);
                OnPropertyChanged();
            }
        }

        private int selectedItemIndex;
        public int SelectedItemIndex
        {
            get { return selectedItemIndex; }
            set
            {
                if (selectedItemIndex != value)
                {
                    selectedItemIndex = value;
                    Console.WriteLine($"EditorVM, selectedIndexUpdated to {value}");
                    OnPropertyChanged(); 
                }
            }
        }

        private AsyncVM asyncVM;
        public AsyncVM AsyncVM
        {
            get { return asyncVM; }
            set { asyncVM = value; OnPropertyChanged(); }
        }

        public static List<CsvFileConfiguration> FileConfigurations { get; set; }

        public ObservableCollection<CsvFile> CsvFiles { get; set; }

        public ObservableCollection<DirectoryWithCsv> CsvFilesStructure { get; set; }

        public static Action<Grid> ConfiguraitonUpdated;
        public Action ConfigurationUpdated;

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
            SwitchEditModeCommand = new DelegateCommand(switchLineEditMode);

            FileConfigurations = initializeFileConfigurations();
            Application.Current.MainWindow.SizeChanged += MainWindow_SizeChanged;
            setAppOptions();
            SetVisuals(AppOptions.VisualConfig);
        }

        //*******************************
        //*********** Methods ***********
        //*******************************

        private static List<CsvFileConfiguration> initializeFileConfigurations()
        {
            List<CsvFileConfiguration> loadedconfiguration = FileSystemServices.LoadFileConfigurationsFile(Path.Combine(ConfigurationFolderPath, CSV_CONFIGURATIONS_FILE_NAME));
            return loadedconfiguration == null ? new List<CsvFileConfiguration>() : loadedconfiguration;
        }

        private void setSelectedFile(string value, bool needsProcessing = true)
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

        private void setSelectedCsvFile(CsvFile value)
        {
            if (AppOptions != null) AppOptions.LastSelectedCsvFile = value;
            if (value != null)
            {
                value.ColumnConfigurations = resolveCsvFileConfiguration(value.ColumnConfigurations, value.AbsPath);
            }
            selectedCsvFile = value;
        }

        private List<CsvColumnConfiguration> resolveCsvFileConfiguration(List<CsvColumnConfiguration> currentFileConfigurations, string currentFileAbsPath)
        {
            return getConfigurationForCurrentFile(currentFileConfigurations, currentFileAbsPath);
        }

        private List<CsvColumnConfiguration> getConfigurationForCurrentFile(List<CsvColumnConfiguration> currentFileConfigurations, string currentFileAbsPath)
        {
            var foundConfiguration = findFileConfiguration(currentFileAbsPath);

            if (foundConfiguration == null)
            {
                addNewFileConfiguration(currentFileConfigurations, currentFileAbsPath);

                return currentFileConfigurations;
            }

            return foundConfiguration != null
                ? foundConfiguration
                : currentFileConfigurations;
        }

        private List<CsvColumnConfiguration> findFileConfiguration(string fileAbsPath)
        {
            return FileConfigurations?
                .Where(conf => conf.AbsoluteFilePath == fileAbsPath)
                .DefaultIfEmpty()?
                .FirstOrDefault()?.ColumnConfigurations;
        }

        public void addNewFileConfiguration(List<CsvColumnConfiguration> currentFileConfigurations, string fileAbsPath)
        {
            FileConfigurations.Add(new CsvFileConfiguration()
            {
                AbsoluteFilePath = fileAbsPath,
                ColumnConfigurations = currentFileConfigurations
            });
        }

        public void UpdateFileConfigurations(Grid mainGridContainer = null)
        {
            if (findFileConfiguration(SelectedCsvFile.AbsPath) == null)
            {
                throw new InvalidDataException("Error in file configuration process - configuration NOT FOUND, which should not happen on update.");
            }

            Console.WriteLine($"Updating configuration for {SelectedCsvFile.AbsPath} in {nameof(FileConfigurations)}");

            var configToUpdateIndex = FileConfigurations.FindIndex(config => config.AbsoluteFilePath == SelectedCsvFile.AbsPath);

            FileConfigurations[configToUpdateIndex].ColumnConfigurations = SelectedCsvFile.ColumnConfigurations;

            ConfigurationUpdated?.Invoke();

            if (mainGridContainer != null)
            {
                ConfiguraitonUpdated?.Invoke(mainGridContainer); 
            }
        }

        private void setAppOptions()
        {
            AppOptions = new AppOptions();

            checkConfigDirectory();

            var loadedOptions = JsonServices.DeserializeJson<AppOptions>(
                Path.Combine(ConfigurationFolderPath, APP_OPTIONS_FILE_NAME),
                "App Options");

            if (loadedOptions != null)
            {
                RootRepositoryPath = loadedOptions.LastRootPath;
                setSelectedFile(loadedOptions.LastSelectedFilePath, false);
                SelectedCsvFile = loadedOptions.LastSelectedCsvFile;
                IsGitRepo = FileSystemServices.IsDirectoryWithGitRepository(RootRepositoryPath);
                CsvFilesStructure.Clear();
                loadedOptions.LastCsvFilesStructure.ForEach(record => CsvFilesStructure.Add(record));
                AppOptions.LastCsvFilesStructure = CsvFilesStructure.ToList(); // Setting whole new CsvFileStructure instead of line by line via CsvFilesStructure OnChange event
                SelectedItemIndex = 0;
                AppOptions.VisualConfig = loadedOptions.VisualConfig;
            }
        }

        private void SetVisuals(VisualConfig visualConfig)
        {
            Application.Current.MainWindow.Width = visualConfig.MainWindowWidth;
            Application.Current.MainWindow.Height = visualConfig.MainWindowHeight;
        }

        private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            AppOptions.VisualConfig.MainWindowWidth = e.NewSize.Width;
            AppOptions.VisualConfig.MainWindowHeight = e.NewSize.Height;
        }

        private void checkConfigDirectory()
        {
            if (!Directory.Exists(Path.Combine(BaseAppPath, CONFIGURATION_FOLDER_NAME)))
            {
                Console.WriteLine($"Creating new {CONFIGURATION_FOLDER_NAME} directory in {BaseAppPath}");
                Directory.CreateDirectory(Path.Combine(BaseAppPath, CONFIGURATION_FOLDER_NAME));
            }
        }

        private void switchLineEditMode()
        {
            setLineEditMode(!IsLineEditMode);
        }

        private void setLineEditMode(bool lineEditMode)
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
