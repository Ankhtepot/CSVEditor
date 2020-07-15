using CSVEditor.Model.HelperClasses;
using CSVEditor.Model.Interfaces;
using LibGit2Sharp;
using Prism.Commands;
using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace CSVEditor.ViewModel
{
    public class GitVM : INotifyPropertyChanged
    {
        public IWindowService WindowService { get; set; }

        private bool isGitRepo;
        public bool IsGitRepo
        {
            get { return isGitRepo; }
            set 
            {
                isGitRepo = value;
                OnPropertyChanged();
            }
        }

        private bool isRepositoryUpToDate;
        public bool IsRepositoryUpToDate
        {
            get { return isRepositoryUpToDate; }
            set 
            {
                isRepositoryUpToDate = value;
                OnPropertyChanged();
            }
        }

        private bool isRepositoryCommited;
        public bool IsRepositoryCommited
        {
            get { return isRepositoryCommited; }
            set 
            {
                isRepositoryCommited = value;
                OnPropertyChanged();
            }
        }

        private bool isRepositoryPushed;
        public bool IsRepositoryPushed
        {
            get { return isRepositoryPushed; }
            set 
            {
                isRepositoryPushed = value;
                OnPropertyChanged();
            }
        }

        private IRepository currentRepository;
        public IRepository CurrentRepository
        {
            get { return currentRepository; }
            set 
            {
                currentRepository = value;
                OnPropertyChanged();
            }
        }

        public DelegateCommand CommitRepositoryCommand { get; set; }
        public DelegateCommand PushRepositoryCommand { get; set; }
        public DelegateCommand PullRepositoryCommand { get; set; }

        public GitVM(IWindowService windowService)
        {
            CommitRepositoryCommand = new DelegateCommand(CommitRepository);
            PushRepositoryCommand = new DelegateCommand(PushRepository);
            PullRepositoryCommand = new DelegateCommand(PullRepository);

            SaveVM.OnSaved += ProcessRepositoryOnSave;
        }

        public void SetGitInfo(string rootRepositoryPath)
        {
            IsGitRepo = FileSystemServices.IsDirectoryWithGitRepository(rootRepositoryPath);
            if (IsGitRepo)
            {
                SetRepository(rootRepositoryPath);
            }
        }

        private void PullRepository()
        {
            throw new NotImplementedException();
        }

        private void PushRepository()
        {
            throw new NotImplementedException();
        }

        private void CommitRepository()
        {
            throw new NotImplementedException();
        }

        public void SetRepository(string path)
        {
            try
            {
                CurrentRepository = new Repository(path);
                var status = IsUnstaged();
            }
            catch (RepositoryNotFoundException e)
            {
                Console.WriteLine($"Path \"{path}\" doesn't contain valid repository. Error: {e.Message}");
            }
        }

        private void ProcessRepositoryOnSave(bool commitOnSave, bool pushOnSave)
        {
            if (commitOnSave)
            {
                var options = EditorVM.AppOptions.GitOptions;
                options = WindowService.OpenGitSetupWindow(options);

                //CurrentRepository.Commit(options.CommitMessage, new Signature()
            }
        }

        public bool IsUnstaged()
        {
            return CurrentRepository?.RetrieveStatus(new StatusOptions()).Count() > 0;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
