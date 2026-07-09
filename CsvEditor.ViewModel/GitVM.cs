using CSVEditor.Model.Interfaces;
using LibGit2Sharp;
using LibGit2Sharp.Handlers;
using Repository = LibGit2Sharp.Repository;
using Signature = LibGit2Sharp.Signature;
using Prism.Commands;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using CSVEditor.Model.Services;
using CSVEditor.Model.HelperClasses;
using Octokit;

namespace CSVEditor.ViewModel
{
    //*************************************************************************
    //************************    testing repo info     ***********************
    //git remote add origin https://github.com/Ankhtepot/CsvEditorTesting.git
    //git to kocourweb: https://github.com/miljed/kocourweb.git
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

        public bool IsLoggedIn => EditorVM.AppOptions?.GitOptions?.IsAuthenticated ?? false;

        public string LoginTooltip => IsLoggedIn
            ? $"Logged in as: {EditorVM.AppOptions.GitOptions.UserName} ({EditorVM.AppOptions.GitOptions.Email})."
            : "Log in in option Git -> Setup Git Login Options";

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

            SubscribeToGitOptions();
        }

        public async Task InitializeAsync()
        {
            if (IsLoggedIn)
            {
                await VerifyGitHubLoginAsync();
            }
        }

        private async Task VerifyGitHubLoginAsync()
        {
            var gitOpts = EditorVM.AppOptions?.GitOptions;
            if (gitOpts == null || !gitOpts.IsAuthenticated) return;

            string token = gitOpts.UseToken ? CredentialService.GetToken(gitOpts.UserName) ?? gitOpts.Password : gitOpts.Password;
            if (string.IsNullOrEmpty(token))
            {
                gitOpts.IsAuthenticated = false;
                return;
            }

            try
            {
                GitHubClient client = new GitHubClient(new ProductHeaderValue("CSVEditor"))
                {
                    Credentials = new Octokit.Credentials(token)
                };
                await client.User.Current();
                // Successfully verified
            }
            catch (AuthorizationException)
            {
                // Token is invalid or expired
                gitOpts.IsAuthenticated = false;
                SaveVM.SaveAppOptions();
            }
            catch (Exception)
            {
                // Network error or other issues - keep the state as is to avoid logging out when offline
            }
        }

        private void SubscribeToGitOptions()
        {
            if (EditorVM.AppOptions?.GitOptions != null)
            {
                EditorVM.AppOptions.GitOptions.PropertyChanged -= GitOptions_PropertyChanged;
                EditorVM.AppOptions.GitOptions.PropertyChanged += GitOptions_PropertyChanged;
            }
        }

        private void GitOptions_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(GitOptions.IsAuthenticated) ||
                e.PropertyName == nameof(GitOptions.UserName) ||
                e.PropertyName == nameof(GitOptions.Email))
            {
                OnPropertyChanged(nameof(IsLoggedIn));
                OnPropertyChanged(nameof(LoginTooltip));
            }
        }

        public void SetGitInfo(string rootRepositoryPath)
        {
            SubscribeToGitOptions();
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
            var oldOptions = EditorVM.AppOptions.GitOptions;
            var newOptions = WindowService?.OpenGitSetupWindow();
            if (newOptions != null)
            {
                if (oldOptions != null)
                {
                    oldOptions.PropertyChanged -= GitOptions_PropertyChanged;
                }
                EditorVM.AppOptions.GitOptions = newOptions;
                SubscribeToGitOptions();
                
                OnPropertyChanged(nameof(IsLoggedIn));
                OnPropertyChanged(nameof(LoginTooltip));

                SaveVM.SaveAppOptions();
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
                using (var repo = new Repository(CurrentRepository.Info.Path))
                {
                    var gitOpts = EditorVM.AppOptions.GitOptions;
                    var password = gitOpts.UseToken ? CredentialService.GetToken(gitOpts.UserName) ?? gitOpts.Password : gitOpts.Password;
                    // Credential information to fetch
                    PullOptions options = new PullOptions();
                    options.FetchOptions = new FetchOptions();
                    options.FetchOptions.CredentialsProvider = new CredentialsHandler(
                        (_, _, _) =>
                            new UsernamePasswordCredentials()
                            {
                                Username = gitOpts.UserName,
                                Password = password
                            });

                    // User information to create a merge commit
                    var signature = new Signature(
                        new Identity("MERGE_USER_NAME", "MERGE_USER_EMAIL"), DateTimeOffset.Now);

                    // Pull
                    Commands.Pull(repo, signature, options);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($@"Error pulling repository. Error: {e.Message}");
            }
        }

        private void PushRepository()
        {
            if (!OpenGitSetupWindow())
            {
                return;
            }

            PushRepositorySolo();
        }

        private void PushRepositorySolo()
        {
            try
            {
                using (var repo = new Repository(CurrentRepository.Info.Path))
                {
                    var gitOpts = EditorVM.AppOptions.GitOptions;
                    var password = gitOpts.UseToken ? CredentialService.GetToken(gitOpts.UserName) ?? gitOpts.Password : gitOpts.Password;

                    var remote = repo.Network.Remotes["origin"];
                    if (remote == null)
                    {
                        if (!string.IsNullOrEmpty(gitOpts.RemoteRepositoryLink) && !gitOpts.RemoteRepositoryLink.Contains("<"))
                        {
                            Console.WriteLine($@"Remote 'origin' not found. Adding remote 'origin' with link: {gitOpts.RemoteRepositoryLink}");
                            remote = repo.Network.Remotes.Add("origin", gitOpts.RemoteRepositoryLink);
                        }
                        else
                        {
                            Console.WriteLine(@"Remote 'origin' not found and no valid remote link provided in Git Setup.");
                            return;
                        }
                    }

                    var options = new PushOptions
                    {
                        CredentialsProvider = (_, _, _) =>
                            new UsernamePasswordCredentials()
                            {
                                Username = gitOpts.UserName,
                                Password = password
                            }
                    };

                    // Push the current branch to origin
                    repo.Network.Push(remote, repo.Head.CanonicalName, options);
                    
                    Console.WriteLine($@"Repository pushed to {remote.Name}.");
                    IsRepositoryPushed = true;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($@"Error pushing repository. Error: {e.Message}");
            }
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
                using (var repo = new Repository(CurrentRepository.Info.Path))
                {
                    if (IsRepositoryUnstaged())
                    {
                        StageRepository();
                        Console.WriteLine($@"Repository staged.");
                    }

                    repo.Commit(options.CommitMessage, authorSifnature, authorSifnature);

                    Console.WriteLine($@"Repository commited");

                    return true;
                }
            }
            catch (EmptyCommitException)
            {
                Console.WriteLine(Properties.Resources.NotCommitedError);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine($@"Unexpected error during commit. Error: {e.Message}");
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
                Console.WriteLine($@"Error while staging repository at ""{CurrentRepository.Info.WorkingDirectory}"". Error: {e.Message}");
                return false;
            }
        }

        public void SetRepository(string path)
        {
            try
            {
                Console.WriteLine(@"[GitVM] Setting up repository.");
                CurrentRepository = new Repository(path);
                var status = IsRepositoryUnstaged();
            }
            catch (RepositoryNotFoundException e)
            {
                Console.WriteLine($@"Path ""{path}"" doesn't contain valid repository. Error: {e.Message}");
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
