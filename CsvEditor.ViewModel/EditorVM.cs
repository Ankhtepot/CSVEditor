using CSVEditor.Model;
using CSVEditor.ViewModel.Abstracts;
using CSVEditor.ViewModel.BackgroundWorkers;
using CSVEditor.ViewModel.Commands;
using JetBrains.Annotations;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using static CSVEditor.Model.Enums;

namespace CSVEditor.ViewModel
{
    public class EditorVM : INotifyPropertyChanged
    {
        public readonly DirectoryWithCsv DEFAULT_DIRECTORY = new DirectoryWithCsv("Directory", new ObservableCollection<string> { "Files..." });

        private string rootRepositoryPath;

        public string RootRepositoryPath
        {
            get => rootRepositoryPath;
            set
            {
                rootRepositoryPath = value; OnPropertyChanged(nameof(RootRepositoryPath));
            }
        }

        private bool isGitRepo;

        public bool IsGitRepo
        {
            get { return isGitRepo; }
            set { isGitRepo = value; OnPropertyChanged(nameof(IsGitRepo)); }
        }

        private WorkStatus workingStatus;

        public WorkStatus WorkingStatus
        {
            get { return workingStatus; }
            set 
            {
                workingStatus = value;
                Console.WriteLine($"EditorVM:Working status = {value}");
                OnPropertyChanged(nameof(WorkingStatus)); 
            }
        }

        private int workProgress;

        public int WorkProgress
        {
            get { return workProgress; }
            set {
                workProgress = value;
                OnPropertyChanged(); }
        }

        private AbstractEditorVMWorker activeWorker;

        public AbstractEditorVMWorker ActiveWorker //this field is being handled by AbstractEditorVMWorker
        {
            get { return activeWorker; }
            set { activeWorker = value; OnPropertyChanged(); }
        }


        private bool isEditing;

        public bool IsEditing
        {
            get { return isEditing; }
            set { isEditing = value; OnPropertyChanged(nameof(IsEditing)); }
        }

        private string selectedText;

        public string SelectedText
        {
            get { return selectedText; }
            set
            {
                selectedText = value; OnPropertyChanged(nameof(SelectedText));
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
                OnPropertyChanged(nameof(SelectedDirectory));
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
                selectedFile = value;
                Console.WriteLine("MainVM.SelectedCsvFile: " + selectedFile);
                ProcessSelectedFile();
                OnPropertyChanged(nameof(SelectedFile));
            }
        }

        private CsvFile selectedCsvFile;

        public CsvFile SelectedCsvFile
        {
            get { return selectedCsvFile; }
            set { selectedCsvFile = value; OnPropertyChanged(); }
        }

        public ObservableCollection<CsvFile> CsvFiles { get; set; }

        public ObservableCollection<DirectoryWithCsv> CsvFilesStructure { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        //*******************************
        //********** Commands ***********
        //*******************************

        public LoadRepositoryCommand LoadRepositoryCommand { get; set; }
        public CancelActiveWorkerAsyncCommand CancelActiveWorkerAsyncCommand { get; set; }

        //*******************************
        //********* Constructor *********
        //*******************************

        public EditorVM()
        {
            RootRepositoryPath = Constants.LOAD_REPOSITORY_PLACEHOLDER;
            IsGitRepo = false;
            SelectedText = Constants.SELECTED_TEXT_DEFAULT;
            WorkingStatus = WorkStatus.Idle;

            LoadRepositoryCommand = new LoadRepositoryCommand(this);
            CancelActiveWorkerAsyncCommand = new CancelActiveWorkerAsyncCommand(this);
            CsvFilesStructure = new ObservableCollection<DirectoryWithCsv>();
            CsvFilesStructure.Add(DEFAULT_DIRECTORY);
        }

        //*******************************
        //*********** Methods ***********
        //*******************************

        public void LoadRepository()
        {
            SelectedFile = null;

            RootRepositoryPath = FileSystemServices.QueryUserForRootRepositoryPath();

            IsGitRepo = FileSystemServices.IsDirectoryWithGitRepository(RootRepositoryPath);

            CsvFilesStructure.Clear();

            new LoadDirectoriesWithCsvWorker(this).RunAsync(RootRepositoryPath);
        }

        public void CancelActiveWorkerAsync()
        {
            ActiveWorker?.CancelAsync();
        }

        public void ProcessSelectedFile()
        {
            if(File.Exists(SelectedFile))
            {
                new GetCsvFileFromPathWorker(this).RunAsync(SelectedFile);
            }
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
