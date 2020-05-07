using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using Microsoft.WindowsAPICodePack.Dialogs;
using CSVEditor.Annotations;
using CSVEditor.Model;
using CSVEditor.ViewModel.Commands;

namespace CSVEditor.ViewModel
{
    public class EditorVM : INotifyPropertyChanged
    {
        private string rootRepositoryPath;

        public string RootRepositoryPath {
            get => rootRepositoryPath;
            set {
                rootRepositoryPath = value;
                OnPropertyChanged(nameof(RootRepositoryPath));
            }
        }

        private bool isGitRepo;

        public bool IsGitRepo {
            get { return isGitRepo; }
            set { isGitRepo = value; OnPropertyChanged(nameof(IsGitRepo)); }
        }

        private DirectoryWithCsv selectedDirectory;

        public DirectoryWithCsv SelectedDirectory {
            get { return selectedDirectory; }
            set {
                selectedDirectory = value;
                Console.WriteLine("Selected Directory: " + selectedDirectory?.DirectoryAbsolutePath);
                OnPropertyChanged(nameof(SelectedDirectory));
            }
        }

        private string selectedCsvFile;

        public string SelectedCsvFile {
            get { return selectedCsvFile; }
            set 
            {
                selectedCsvFile = value;
                Console.WriteLine("MainVM.SelectedCsvFile: " + selectedCsvFile);
                OnPropertyChanged(nameof(SelectedCsvFile));
            }
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

            LoadRepositoryCommand = new LoadRepositoryCommand(this);
            CsvFilesStructure = new ObservableCollection<DirectoryWithCsv>();
            CsvFilesStructure.Add(new DirectoryWithCsv("Directory", new ObservableCollection<string> { "Files..." }));
        }

        //*******************************
        //*********** Methods ***********
        //*******************************

        public void LoadRepository()
        {
            RootRepositoryPath = FileSystemServices.LoadRootRepositoryPath();

            IsGitRepo = FileSystemServices.IsDirectoryWithGitRepository(RootRepositoryPath);

            CsvFilesStructure.Clear();
            foreach (var directory in FileSystemServices.GetCsvFilesStructureFromRootDirectory(RootRepositoryPath))
            {
                CsvFilesStructure.Add(directory);
            }
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
