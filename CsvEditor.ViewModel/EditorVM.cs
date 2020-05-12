using CSVEditor.Model;
using CSVEditor.ViewModel.Commands;
using JetBrains.Annotations;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using static CSVEditor.Model.Enums;

namespace CSVEditor.ViewModel
{
    public class EditorVM : INotifyPropertyChanged
    {
        private readonly DirectoryWithCsv DEFAULT_DIRECTORY = new DirectoryWithCsv("Directory", new ObservableCollection<string> { "Files..." });

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
            set { workingStatus = value; OnPropertyChanged(); }
        }

        private int workProgress;

        public int WorkProgress
        {
            get { return workProgress; }
            set { workProgress = value; OnPropertyChanged(); }
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
                SelectedCsvFile = new CsvFile(selectedFile);
                OnPropertyChanged(nameof(SelectedFile));
            }
        }

        private CsvFile selectedCsvFile;

        public CsvFile SelectedCsvFile
        {
            get { return selectedCsvFile; }
            set { selectedCsvFile = value; }
        }



        public ObservableCollection<CsvFile> CsvFiles { get; set; }

        public ObservableCollection<DirectoryWithCsv> CsvFilesStructure { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        //*******************************
        //********** Commands ***********
        //*******************************

        public LoadRepositoryCommand LoadRepositoryCommand { get; set; }

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
            CsvFilesStructure = new ObservableCollection<DirectoryWithCsv>();
            CsvFilesStructure.Add(DEFAULT_DIRECTORY);
        }

        //*******************************
        //*********** Methods ***********
        //*******************************

        public void LoadRepository()
        {
            RootRepositoryPath = FileSystemServices.LoadRootRepositoryPath();

            IsGitRepo = FileSystemServices.IsDirectoryWithGitRepository(RootRepositoryPath);

            CsvFilesStructure.Clear();

            //var foundStructure = FileSystemServices.GetCsvFilesStructureFromRootDirectory(RootRepositoryPath);
            //if (foundStructure != null && foundStructure.Count > 0)
            //{
            //    foreach (var directory in foundStructure)
            //    {
            //        CsvFilesStructure.Add(directory);
            //    }
            //}
            //else
            //{
            //    CsvFilesStructure.Add(DEFAULT_DIRECTORY);
            //}
            
            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += loadDirectoryStructure_DoWork;
            worker.WorkerReportsProgress = true;
            worker.ProgressChanged += loadDirectoryStructure_ProgressChanged;
            worker.RunWorkerCompleted += loadDirectoryStructure_Completed;
            worker.RunWorkerAsync(RootRepositoryPath);
            
        }

        private void loadDirectoryStructure_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = (BackgroundWorker)sender;
            worker.ReportProgress(100);

            WorkingStatus = WorkStatus.Working;

            var path = e.Argument as string;
            ObservableCollection<DirectoryWithCsv> result = new ObservableCollection<DirectoryWithCsv>();

            result = FileSystemServices.CrawlDirectoryForCsvFiles(result, path, path);

            WorkingStatus = WorkStatus.Idle;
        }

        private void loadDirectoryStructure_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e.UserState == null)
            {
                WorkProgress = e.ProgressPercentage;
                return;
            }

            var foundStructure = e.UserState as ObservableCollection<DirectoryWithCsv>;
            if (foundStructure != null && foundStructure.Count > 0)
            {
                foreach (var directory in foundStructure)
                {
                    CsvFilesStructure.Add(directory);
                }
            }
            else
            {
                CsvFilesStructure.Add(DEFAULT_DIRECTORY);
            }
        }

        private void loadDirectoryStructure_Completed(object sender, RunWorkerCompletedEventArgs e)
        {
            var resultLabel = "";
            if (e.Cancelled == true)
            {
                resultLabel = "Canceled!";
            }
            else if (e.Error != null)
            {
                resultLabel = "Error: " + e.Error.Message;
            }
            else
            {
                resultLabel = "Done!";
            }

            Console.WriteLine($"BW-Completed status: {resultLabel}");
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
