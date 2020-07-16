using CSVEditor.Model.Interfaces;
using LibGit2Sharp;
using Prism.Commands;
using System;
using System.ComponentModel;
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

        private Repository currentRepository;
        public Repository CurrentRepository
        {
            get { return currentRepository; }
            set 
            {
                currentRepository = value;
                OnPropertyChanged();
            }
        }

        public DelegateCommand OpenGitSetupCommand { get; set; }
        public DelegateCommand CommitRepositoryCommand { get; set; }
        public DelegateCommand PushRepositoryCommand { get; set; }
        public DelegateCommand PullRepositoryCommand { get; set; }

        public GitVM(IWindowService windowService)
        {
            WindowService = windowService;
            OpenGitSetupCommand = new DelegateCommand(OpenGitSetup);
            CommitRepositoryCommand = new DelegateCommand(CommitRepositorySolo);
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

        private void OpenGitSetup()
        {
            OpenGitSetupWindow();
        }

        private bool OpenGitSetupWindow()
        {
            var newOptions = WindowService?.OpenGitSetupWindow();
            if (newOptions != null)
            {
                EditorVM.AppOptions.GitOptions = newOptions;
                return true;
            }

            return false;
        }

        private void PullRepository()
        {
            throw new NotImplementedException();
        }

        private void PushRepository()
        {
            throw new NotImplementedException();
        }

        private void CommitRepositorySolo()
        {
            if (!OpenGitSetupWindow())
            {
                return;
            }

            CommitRepository();
        }

        private void CommitRepository()
        {
            var options = EditorVM.AppOptions.GitOptions;

            var authorSifnature = new Signature(options.Author, options.Email, DateTimeOffset.Now);

            try
            {
                if (IsRepositoryUnstaged())
                {

                }

                CurrentRepository.Commit(options.CommitMessage, authorSifnature, authorSifnature);
            }
            catch (EmptyCommitException)
            {
                Console.WriteLine(Properties.Resources.NotCommitedError);
            }
        }

        private void StageRepository()
        {
            using (var repo = new Repository(CurrentRepository.Info.WorkingDirectory))
            {
                
            }
        }

        public void SetRepository(string path)
        {
            try
            {
                Console.WriteLine("[GitVM] Setting up repository.");
                CurrentRepository = new Repository(path);
                var status = IsRepositoryUnstaged();
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
                if (!OpenGitSetupWindow())
                {
                    return;
                }

                CommitRepository();
            }
        }

        public bool IsRepositoryUnstaged()
        {
            return CurrentRepository?.RetrieveStatus().IsDirty == true;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
