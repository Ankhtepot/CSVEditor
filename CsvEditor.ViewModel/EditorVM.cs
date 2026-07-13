using CSVEditor.Core;
using CSVEditor.Core.HelperClasses;
using CSVEditor.Core.Interfaces;
using CSVEditor.Core.Services;
using CSVEditor.ViewModel.BackgroundWorkers;
using CSVEditor.Core.Properties;
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
using CSVEditor.Core.Extensions;
using static CSVEditor.Core.HelperClasses.Enums;
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace CSVEditor.ViewModel
{
    public class EditorVM : INotifyPropertyChanged
    {
        public readonly DirectoryWithCsv DEFAULT_DIRECTORY =
            new(Resources.DefaultDirectoryName, [Resources.DefaultFilesName]);

        public static string ConfigurationFolderPath => FileSystemService.ConfigurationFolderPath;

        public bool ShouldExitAfterSave;

        public IWindowService WindowService { get; set; }

        private static AppOptions AppOptions => AppOptionsService.AppOptions;

        public string RootRepositoryPath
        {
            get;
            set
            {
                field = value;
                if (value != null && AppOptions != null) AppOptions.LastRootPath = value;
                OnPropertyChanged();
            }
        }

        public bool IsLineEditMode
        {
            get;
            set
            {
                if (field != value)
                {
                    field = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsFileEdited
        {
            get;
            set
            {
                field = value;
                AppOptions?.WasEdited = value;
                OnPropertyChanged();
            }
        }

        public string SelectedText
        {
            get;
            set
            {
                field = value;
                OnPropertyChanged();
            }
        }

        private string _selectedFile;

        public string SelectedFile
        {
            get => string.IsNullOrEmpty(_selectedFile) ? Constants.NO_FILE_SELECTED : _selectedFile;
            set
            {
                SetSelectedFile(value);
                OnPropertyChanged();
            }
        }

        private CsvFile selectedCsvFile;

        public CsvFile SelectedCsvFile
        {
            get => selectedCsvFile;
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

        public int SelectedItemIndex
        {
            get;
            set
            {
                if (field == value) return;

                field = value;
                Console.WriteLine(Resources.SelectedIndexUpdatedFormat, value);
                OnPropertyChanged();
            }
        }

        public AsyncVM AsyncVM { get; set; }
        public GitVM GitVM { get; set; }

        public static List<CsvFileConfiguration> FileConfigurations { get; set; }

        public ObservableCollection<CsvFile> CsvFiles { get; set; }

        public ObservableCollection<DirectoryWithCsv> CsvFilesStructure { get; set; }

        public Dictionary<AddLinePlacement, string> AddLinePlacementSource { get; set; }

        public static Action<Grid> OnGridConfigurationUpdated;
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
        public DelegateCommand<string> AddLineCommand { get; set; }
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

        public EditorVM(IWindowService windowService)
        {
            WindowService = windowService;
            AsyncVM = new AsyncVM(this);
            GitVM = new GitVM(this);

            RootRepositoryPath = Constants.LOAD_REPOSITORY_PLACEHOLDER;
            IsLineEditMode = true;
            IsFileEdited = false;
            GitVM.IsGitRepo = false;
            ShouldExitAfterSave = false;
            SelectedText = Constants.SELECTED_TEXT_DEFAULT;
            AsyncVM.WorkingStatus = WorkStatus.Idle;
            CsvFilesStructure =
            [
                DEFAULT_DIRECTORY
            ];
            AddLinePlacementSource = GetAddLineComboBoxSource();

            LoadRepositoryCommand = new DelegateCommand(AsyncVM.LoadRepository, AsyncVM.LoadRepository_CanExecute);
            CancelActiveWorkerAsyncCommand = new DelegateCommand(AsyncVM.CancelActiveWorkerAsync);
            SwitchEditModeCommand = new DelegateCommand(SwitchLineEditMode);
            AddLineCommand = new DelegateCommand<string>(AddLine);
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
            Application.Current.MainWindow?.SizeChanged += MainWindow_SizeChanged;
            SetAppOptions();
            GitVM.SetGitInfo(RootRepositoryPath);
            _ = GitVM.InitializeAsync();
            SetVisuals(AppOptions.VisualConfig);
        }

        //*******************************
        //*********** Methods ***********
        //*******************************        

        public void UpdateFileConfigurations(Grid mainGridContainer = null)
        {
            if (FindFileConfiguration(SelectedCsvFile.AbsPath) == null)
            {
                throw new InvalidDataException(Resources.FileConfigurationNotFoundException);
            }

            Console.WriteLine(Resources.UpdatingConfigurationFormat, SelectedCsvFile.AbsPath, nameof(FileConfigurations));

            int configToUpdateIndex =
                FileConfigurations.FindIndex(config => config.AbsoluteFilePath == SelectedCsvFile.AbsPath);

            FileConfigurations[configToUpdateIndex].ColumnConfigurations = SelectedCsvFile.ColumnConfigurations;

            if (mainGridContainer != null)
            {
                OnGridConfigurationUpdated?.Invoke(mainGridContainer);
            }
            else
            {
                OnConfigurationUpdated?.Invoke();
            }
        }

        public void OnWindowClosing(object sender, CancelEventArgs e)
        {
            if (IsFileEdited)
            {
                MessageBoxResult result = MessageBoxHelper.ShowQueryYesNoCancelBox(Constants.SAVE_FILE_TITLE,
                    Constants.SAVE_FILE_BEFORE_EXIT_QUERY);

                if (result == MessageBoxResult.Yes)
                {
                    e.Cancel = true;
                    ShouldExitAfterSave = true;
                    SaveCurrentCsvFile();
                }

                if (result == MessageBoxResult.Cancel)
                {
                    e.Cancel = true;
                    return;
                }
            }

            SaveVM.SaveConfiguration();
            AppOptionsService.SaveAppOptions();
        }

        private Dictionary<AddLinePlacement, string> GetAddLineComboBoxSource()
        {
            return new Dictionary<AddLinePlacement, string>()
            {
                {AddLinePlacement.ToTheTop, Resources.AddLineToTop},
                {AddLinePlacement.Above, Resources.AddLineAbove},
                {AddLinePlacement.Below, Resources.AddLineAbove},
                {AddLinePlacement.ToTheBottom, Resources.AddLineToBottom}
            };
        }

        private void AddNewFileConfiguration(List<CsvColumnConfiguration> currentFileConfigurations, string fileAbsPath)
        {
            FileConfigurations.Add(new CsvFileConfiguration()
            {
                AbsoluteFilePath = fileAbsPath,
                ColumnConfigurations = currentFileConfigurations
            });
        }

        private void SaveCurrentCsvFile()
        {
            SaveVM.SaveCurrentCsvFile(this, WindowService);
        }

        private void SaveAndExit()
        {
            ShouldExitAfterSave = true;
            SaveCurrentCsvFile();
        }

        private void ExitApp()
        {
            if (IsFileEdited)
            {
                MessageBoxResult result = MessageBoxHelper.ShowQueryYesNoCancelBox(Constants.SAVE_FILE_TITLE,
                    Constants.SAVE_FILE_BEFORE_EXIT_QUERY);

                switch (result)
                {
                    case MessageBoxResult.Yes:
                        ShouldExitAfterSave = true;
                        SaveCurrentCsvFile();
                        break;
                    case MessageBoxResult.Cancel:
                    default: return;
                }
            }
            else
            {
                Application.Current?.MainWindow?.Close();
            }
        }

        private static List<CsvFileConfiguration> initializeFileConfigurations()
        {
            List<CsvFileConfiguration> loadedConfiguration =
                FileSystemService.LoadFileConfigurationsFile(Path.Combine(ConfigurationFolderPath,
                    Constants.CSV_CONFIGURATIONS_FILE_NAME));
            return loadedConfiguration ?? [];
        }

        private void SetSelectedFile(string value, bool needsProcessing = true)
        {
            _selectedFile = value;

            if (File.Exists(value))
            {
                Console.WriteLine(Resources.SelectedCsvFileLogFormat, _selectedFile);
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
            AppOptions?.LastSelectedCsvFile = value;
            value?.ColumnConfigurations = ResolveCsvFileConfiguration(value.ColumnConfigurations, value.AbsPath);

            selectedCsvFile = value;
            IsLineEditMode = true;
            IsFileEdited = false;

            OnCsvFileSet?.Invoke();
        }

        private List<CsvColumnConfiguration> ResolveCsvFileConfiguration(
            List<CsvColumnConfiguration> currentFileConfigurations, string currentFileAbsPath)
        {
            return GetConfigurationForCurrentFile(currentFileConfigurations, currentFileAbsPath);
        }

        private List<CsvColumnConfiguration> GetConfigurationForCurrentFile(
            List<CsvColumnConfiguration> currentFileConfigurations, string currentFileAbsPath)
        {
            List<CsvColumnConfiguration> foundConfiguration = FindFileConfiguration(currentFileAbsPath);

            if (foundConfiguration == null)
            {
                AddNewFileConfiguration(currentFileConfigurations, currentFileAbsPath);

                return currentFileConfigurations;
            }

            return foundConfiguration;
        }

        private List<CsvColumnConfiguration> FindFileConfiguration(string fileAbsPath)
        {
            return FileConfigurations?
                .Where(conf => conf.AbsoluteFilePath == fileAbsPath)
                .DefaultIfEmpty()
                .FirstOrDefault()?.ColumnConfigurations;
        }
        
        private static async Task VerifyGitHubLoginAsync()
        {
            GitOptions gitOpts = AppOptionsService.AppOptions?.GitOptions;
            if (gitOpts is not {IsAuthenticated: true}) return;

            if (await GitService.VerifyGitHubLoginAsync() is false)
            {
                // Token is invalid or expired
                gitOpts.IsAuthenticated = false;
                AppOptionsService.SaveAppOptions();
            }
            // null means network error - keep state as is to avoid logging out when offline
        }

        private async void SetAppOptions()
        {
            try
            {
                FileSystemService.ValidateConfigDirectory(FileSystemService.BaseAppPath, Constants.CONFIGURATION_FOLDER_NAME);

                AppOptions loadedOptions = AppOptionsService.LoadAppOptions();

                if (loadedOptions == null)
                {
                    Console.WriteLine(Resources.LoadOptionsErrorCreatingNewMessage);
                    loadedOptions = new AppOptions();
                    AppOptionsService.SaveAppOptions();
                }
            
                RootRepositoryPath = loadedOptions.LastRootPath;

                SetSelectedFile(loadedOptions.LastSelectedFilePath, false);
                SelectedCsvFile = loadedOptions.LastSelectedCsvFile;
                IsFileEdited = loadedOptions.WasEdited;
                CsvFilesStructure.Clear();
                loadedOptions.LastCsvFilesStructure.ForEach(record => CsvFilesStructure.Add(record));
                AppOptions.LastCsvFilesStructure =
                    CsvFilesStructure
                        .ToList(); // Setting whole new CsvFileStructure instead of line by line via CsvFilesStructure OnChange event
                AppOptions.VisualConfig = loadedOptions.VisualConfig;
                AppOptions.SaveOptions = loadedOptions.SaveOptions;
                AppOptionsService.SetGitOptions(loadedOptions.GitOptions);
            
                await VerifyGitHubLoginAsync();
            }
            catch (Exception e)
            {
                string errorMessage = string.Format(Resources.AppOptionsProcessingError, e.Message);
                Console.WriteLine(errorMessage);
                AppOptionsService.SetDefaultAppOptions();
            }
        }

        private void SetVisuals(VisualConfig visualConfig)
        {
            Application.Current?.MainWindow?.Width = visualConfig.MainWindowWidth;
            Application.Current?.MainWindow?.Height = visualConfig.MainWindowHeight;
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

        private void AddLine(string addLinePlacement)
        {
            if (addLinePlacement == Resources.AddLineToTop) AddLineToTop();
            else if (addLinePlacement == Resources.AddLineAbove) AddLineUp();
            else if (addLinePlacement == Resources.AddLineBelow) AddLineDown();
            else if (addLinePlacement == Resources.AddLineToBottom) AddLineToBottom();
            else throw new InvalidEnumArgumentException();
        }

        private void AddLine(int index)
        {
            List<string> newLine = [];

            for (int i = 0; i < SelectedCsvFile.ColumnCount; i++)
            {
                newLine.Add("");
            }

            CsvFile updatedCsvFile = new(SelectedCsvFile);
            updatedCsvFile.Lines.Insert(index, newLine);
            SelectedCsvFile = updatedCsvFile;
            SelectedItemIndex = index;
            IsFileEdited = true;

            RequestChangeTab?.Invoke(1);
        }

        private void DeleteLine(int index)
        {
            CsvFile updatedCsvFile = new(SelectedCsvFile);
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
