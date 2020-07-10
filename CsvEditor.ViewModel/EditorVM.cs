using CSVEditor.Model;
using CSVEditor.Model.HelperClasses;
using CSVEditor.Model.Interfaces;
using CSVEditor.Model.Services;
using CSVEditor.Services;
using CSVEditor.ViewModel.BackgroundWorkers;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using static CSVEditor.Model.HelperClasses.Enums;
using static CSVEditor.Model.HelperClasses.SaveOptions;

namespace CSVEditor.ViewModel
{
    public class EditorVM : INotifyPropertyChanged
    {
        public const string APP_OPTIONS_FILE_NAME = "options.json";
        public const string CSV_CONFIGURATIONS_FILE_NAME = "csv_conf.json";
        public const string CONFIGURATION_FOLDER_NAME = "config";

        public readonly DirectoryWithCsv DEFAULT_DIRECTORY = new DirectoryWithCsv("Directory", new List<string> { "Files..." });

        private string rootRepositoryPath;

        public IWindowService WindowService { get; set; }

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

        private bool isFileEdited;
        public bool IsFileEdited
        {
            get { return isFileEdited; }
            set
            {
                isFileEdited = value;
                OnPropertyChanged();
            }
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

                if (value == null)
                {
                    RequestChangeTab?.Invoke(0);
                }

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

        public static Action<Grid> OnConfiguraitonUpdated;
        public Action OnConfigurationUpdated;
        public Action OnCsvFileSet;
        public Action OnCsvFileUpdated;
        public Action<int> RequestChangeTab;

        //*******************************
        //********** Commands ***********
        //*******************************

        public DelegateCommand LoadRepositoryCommand { get; set; }
        public DelegateCommand CancelActiveWorkerAsyncCommand { get; set; }
        public DelegateCommand SwitchEditModeCommand { get; set; }
        public DelegateCommand AddLineUpCommand { get; set; }
        public DelegateCommand AddLineDownCommand { get; set; }
        public DelegateCommand AddLineToTopCommand { get; set; }
        public DelegateCommand AddLineToBottomCommand { get; set; }
        public DelegateCommand EditLineCommand { get; set; }
        public DelegateCommand DeleteLineCommand { get; set; }
        public DelegateCommand SaveCommand { get; set; }
        public DelegateCommand SaveAndExitCommand { get; set; }
        public DelegateCommand ExitCommand { get; set; }

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
            IsFileEdited = false;
            IsGitRepo = false;
            SelectedText = Constants.SELECTED_TEXT_DEFAULT;
            AsyncVM.WorkingStatus = WorkStatus.Idle;
            CsvFilesStructure = new ObservableCollection<DirectoryWithCsv>();
            CsvFilesStructure.Add(DEFAULT_DIRECTORY);

            LoadRepositoryCommand = new DelegateCommand(AsyncVM.LoadRepository, AsyncVM.LoadRepository_CanExecute);
            CancelActiveWorkerAsyncCommand = new DelegateCommand(AsyncVM.CancelActiveWorkerAsync);
            SwitchEditModeCommand = new DelegateCommand(SwitchLineEditMode);
            AddLineUpCommand = new DelegateCommand(AddLineUp);
            AddLineDownCommand = new DelegateCommand(AddLineDown);
            AddLineToTopCommand = new DelegateCommand(AddLineToTop);
            AddLineToBottomCommand = new DelegateCommand(AddLineToBottom);
            EditLineCommand = new DelegateCommand(EditLine);
            DeleteLineCommand = new DelegateCommand(DeleteLine);
            SaveCommand = new DelegateCommand(SaveCurrentCsvFile);
            SaveAndExitCommand = new DelegateCommand(SaveAndExit);
            ExitCommand = new DelegateCommand(ExitApp);

            FileConfigurations = initializeFileConfigurations();
            Application.Current.MainWindow.SizeChanged += MainWindow_SizeChanged;
            SetAppOptions();
            SetVisuals(AppOptions.VisualConfig);
        }

        //*******************************
        //*********** Methods ***********
        //*******************************

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
            if (FindFileConfiguration(SelectedCsvFile.AbsPath) == null)
            {
                throw new InvalidDataException("Error in file configuration process - configuration NOT FOUND, which should not happen on update.");
            }

            Console.WriteLine($"Updating configuration for {SelectedCsvFile.AbsPath} in {nameof(FileConfigurations)}");

            var configToUpdateIndex = FileConfigurations.FindIndex(config => config.AbsoluteFilePath == SelectedCsvFile.AbsPath);

            FileConfigurations[configToUpdateIndex].ColumnConfigurations = SelectedCsvFile.ColumnConfigurations;

            if (mainGridContainer != null)
            {
                OnConfiguraitonUpdated?.Invoke(mainGridContainer);
            }
            else
            {
                OnConfigurationUpdated?.Invoke();
            }
        }

        public async void SaveCurrentCsvFile()
        {
            var saveOptions = AppOptions.SaveOptions;
            var csvText = await AsyncVM.CsvFileToTextTask(SelectedCsvFile);

            var newSaveOptions = WindowService.OpenSaveWindow(saveOptions, csvText, SelectedCsvFile.AbsPath);
        }

        public static void SaveConfiguration()
        {
            JsonServices.SerializeJson(FileConfigurations,
                Path.Combine(ConfigurationFolderPath, CSV_CONFIGURATIONS_FILE_NAME),
                "Csv file configurations");
        }

        public static void SaveAppOptions()
        {
            JsonServices.SerializeJson(AppOptions,
                Path.Combine(ConfigurationFolderPath, APP_OPTIONS_FILE_NAME),
                "App Options");
        }

        public void SaveOnExit()
        {
            SaveCurrentCsvFile();
            ExitApp();
        }

        public void OnWindowClosing(object sender, CancelEventArgs e)
        {
            ExitApp();
        }

        private void SaveAndExit()
        {
            SaveCurrentCsvFile();
            ExitApp();
        }

        private void ExitApp()
        {
            if (IsFileEdited)
            {
                //TODO check if hes sure to not save
            }

            SaveConfiguration();
            SaveAppOptions();
            Application.Current.Shutdown();
        }

        private static List<CsvFileConfiguration> initializeFileConfigurations()
        {
            List<CsvFileConfiguration> loadedconfiguration = FileSystemServices.LoadFileConfigurationsFile(Path.Combine(ConfigurationFolderPath, CSV_CONFIGURATIONS_FILE_NAME));
            return loadedconfiguration == null ? new List<CsvFileConfiguration>() : loadedconfiguration;
        }

        private void SetSelectedFile(string value, bool needsProcessing = true)
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

                RequestChangeTab?.Invoke(0);
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
            if (value != null)
            {
                value.ColumnConfigurations = ResolveCsvFileConfiguration(value.ColumnConfigurations, value.AbsPath);
            }
            selectedCsvFile = value;
            IsLineEditMode = false;
            IsFileEdited = false;

            OnCsvFileSet?.Invoke();
        }

        private List<CsvColumnConfiguration> ResolveCsvFileConfiguration(List<CsvColumnConfiguration> currentFileConfigurations, string currentFileAbsPath)
        {
            return GetConfigurationForCurrentFile(currentFileConfigurations, currentFileAbsPath);
        }

        private List<CsvColumnConfiguration> GetConfigurationForCurrentFile(List<CsvColumnConfiguration> currentFileConfigurations, string currentFileAbsPath)
        {
            var foundConfiguration = FindFileConfiguration(currentFileAbsPath);

            if (foundConfiguration == null)
            {
                addNewFileConfiguration(currentFileConfigurations, currentFileAbsPath);

                return currentFileConfigurations;
            }

            return foundConfiguration != null
                ? foundConfiguration
                : currentFileConfigurations;
        }

        private List<CsvColumnConfiguration> FindFileConfiguration(string fileAbsPath)
        {
            return FileConfigurations?
                .Where(conf => conf.AbsoluteFilePath == fileAbsPath)
                .DefaultIfEmpty()?
                .FirstOrDefault()?.ColumnConfigurations;
        }

        private void SetAppOptions()
        {
            AppOptions = new AppOptions();

            FileSystemServices.ValidateConfigDirectory(BaseAppPath, CONFIGURATION_FOLDER_NAME);

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
                AppOptions.VisualConfig = loadedOptions.VisualConfig ?? new VisualConfig();
                AppOptions.SaveOptions = loadedOptions.SaveOptions ?? new SaveOptions();
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

        private void SwitchLineEditMode()
        {
            SetLineEditMode(!IsLineEditMode);
        }

        private void SetLineEditMode(bool lineEditMode)
        {
            IsLineEditMode = lineEditMode;
        }

        private void DeleteLine()
        {
            DeleteLine(SelectedItemIndex);
        }

        private void EditLine()
        {
            RequestChangeTab?.Invoke(1);
        }

        private void AddLineUp()
        {
            AddLine(SelectedItemIndex);
        }

        private void AddLineDown()
        {
            AddLine(SelectedItemIndex + 1);
        }

        private void AddLineToBottom()
        {
            AddLine(SelectedCsvFile.Lines.Count);
        }

        private void AddLineToTop()
        {
            AddLine(0);
        }

        private void AddLine(int index)
        {
            List<string> newLine = new List<string>();

            for (int i = 0; i < SelectedCsvFile.ColumnCount; i++)
            {
                newLine.Add("");
            }

            var updatedCsvFile = new CsvFile(SelectedCsvFile);
            updatedCsvFile.Lines.Insert(index, newLine);
            SelectedCsvFile = updatedCsvFile;
            SelectedItemIndex = index;

            RequestChangeTab?.Invoke(1);
        }

        private void DeleteLine(int index)
        {
            var updatedCsvFile = new CsvFile(SelectedCsvFile);
            updatedCsvFile.Lines.RemoveAt(index);
            SelectedCsvFile = updatedCsvFile;
            SelectedItemIndex = index.Clamp(0, updatedCsvFile.Lines.Count - 1);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
