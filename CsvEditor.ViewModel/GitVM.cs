using CSVEditor.Model.Interfaces;
using LibGit2Sharp;
using LibGit2Sharp.Handlers;
using Prism.Commands;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace CSVEditor.ViewModel
{
    //*************************************************************************
    //************************    testing repo info     ***********************
    //git remote add origin https://github.com/Ankhtepot/CsvEditorTesting.git
    //git push -u origin master
    //*************************************************************************

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

        public GitVM(EditorVM EditorVM)
        {
            WindowService = EditorVM.WindowService;
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
            if (!OpenGitSetupWindow())
            {
                return;
            }

            PullRepositorySolo();
        }

        private async void PullRepositorySolo()
        {
            try
            {
                var gitOptions = EditorVM.AppOptions.GitOptions;
                var options = new PullOptions();
                options.FetchOptions = new FetchOptions();
                options.FetchOptions.CredentialsProvider = new CredentialsHandler(
                    (url, usernameFromUrl, types) =>
                        new DefaultCredentials()
                        //{
                        //    Username = gitOptions.UserName,
                        //    Password = gitOptions.Password
                        //});
                        );

                var signature = new Signature(
                    new Identity(gitOptions.UserName, gitOptions.Email), DateTimeOffset.Now);

                var result = await Task.Run(() => PullAsync(CurrentRepository, signature, options));
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error pulling repository. Error: {e.Message}");
            }
        }

        private async Task<MergeResult> PullAsync(Repository repository, Signature signature, PullOptions options)
        {
            try
            {
                return await Task.Run(() => Commands.Pull(CurrentRepository, signature, options));
            }
            catch (Exception e)
            {

                throw e;
            }
        }

        private void PushRepository()
        {
            
        }

        private void CommitRepositorySolo()
        {
            if (!OpenGitSetupWindow())
            {
                return;
            }

            CommitRepository();
        }

        private bool CommitRepository()
        {
            var options = EditorVM.AppOptions.GitOptions;

            var authorSifnature = new Signature(options.UserName, options.Email, DateTimeOffset.Now);

            try
            {
                if (IsRepositoryUnstaged())
                {
                    StageRepository();
                    Console.WriteLine($"Repository staged.");
                }

                CurrentRepository.Commit(options.CommitMessage, authorSifnature, authorSifnature);

                Console.WriteLine($"Repository commited");

                return true;
            }
            catch (EmptyCommitException)
            {
                Console.WriteLine(Properties.Resources.NotCommitedError);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Unexpected error during commit. Error: {e.Message}");
                return false;
            }
        }

        private bool StageRepository()
        {
            try
            {
                Commands.Stage(CurrentRepository, "*");
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error while staging repository at \"{CurrentRepository.Info.WorkingDirectory}\". Error: {e.Message}");
                return false;
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

                var commited = CommitRepository();

                if (pushOnSave && commited)
                {
                    PushRepository();
                }
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
