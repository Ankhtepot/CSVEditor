using CSVEditor.Core.Interfaces;
using LibGit2Sharp;
using Repository = LibGit2Sharp.Repository;
using Signature = LibGit2Sharp.Signature;
using Prism.Commands;
using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using CSVEditor.Core.Services;
using CSVEditor.Core.HelperClasses;
using CSVEditor.Core.Properties;
using CSVEditor.Core.Extensions;
using Octokit;
using Branch = LibGit2Sharp.Branch;

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

        public bool IsGitRepo
        {
            get => field;
            set
            {
                field = value;
                OnPropertyChanged();
            }
        }

        public bool IsRepositoryUpToDate
        {
            get => field;
            set
            {
                field = value;
                IsRepositoryUpToDateStatic = value;
                OnPropertyChanged();
            }
        }
        
        public static bool IsRepositoryUpToDateStatic { get; set; }

        public bool IsRepositoryCommited
        {
            get => field;
            set
            {
                field = value;
                OnPropertyChanged();
            }
        }

        public bool IsRepositoryPushed
        {
            get => field;
            set
            {
                field = value;
                OnPropertyChanged();
            }
        }

        public Repository CurrentRepository
        {
            get => field;
            set
            {
                field = value;
                CurrentRepositoryStatic = value;
                OnPropertyChanged();
            }
        }
        
        public static Repository CurrentRepositoryStatic { get; set; }

        public bool IsLoggedIn => EditorVM.AppOptions?.GitOptions?.IsAuthenticated ?? false;

        public string LoginTooltip => IsLoggedIn
            ? string.Format(Resources.LoggedInAsFormat, EditorVM.AppOptions.GitOptions.UserName,
                EditorVM.AppOptions.GitOptions.Email)
            : Resources.LogInHelpText;

        public DelegateCommand OpenGitSetupCommand { get; set; }
        public DelegateCommand CommitRepositoryCommand { get; set; }
        public static DelegateCommand PushRepositoryCommand { get; set; }
        public DelegateCommand PullRepositoryCommand { get; set; }

        public GitVM(EditorVM EditorVM)
        {
            WindowService = EditorVM.WindowService;
            OpenGitSetupCommand = new DelegateCommand(OpenGitSetup);
            CommitRepositoryCommand = new DelegateCommand(CommitRepositoryWithSetup);
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
            GitOptions gitOpts = EditorVM.AppOptions?.GitOptions;
            if (gitOpts == null || !gitOpts.IsAuthenticated) return;

            string token = gitOpts.UseToken
                ? CredentialService.GetToken(gitOpts.UserName) ?? gitOpts.Password
                : gitOpts.Password;
            if (string.IsNullOrEmpty(token))
            {
                gitOpts.IsAuthenticated = false;
                return;
            }

            try
            {
                GitHubClient client = new(new ProductHeaderValue("CSVEditor"))
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
                EditorVM.AppOptions.GitOptions.PropertyChanged -=
                    GitOptions_PropertyChanged; // Unsubscribe first to avoid multiple subscriptions
                EditorVM.AppOptions.GitOptions.PropertyChanged += GitOptions_PropertyChanged;
            }
        }

        private void GitOptions_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName is nameof(GitOptions.IsAuthenticated)
                or nameof(GitOptions.UserName)
                or nameof(GitOptions.Email))
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
            GitOptions oldOptions = EditorVM.AppOptions.GitOptions;
            GitOptions newOptions = WindowService?.OpenGitSetupWindow();

            if (newOptions == null) return false;

            oldOptions.PropertyChanged -= GitOptions_PropertyChanged;

            EditorVM.AppOptions.GitOptions = newOptions;
            SubscribeToGitOptions();

            OnPropertyChanged(nameof(IsLoggedIn));
            OnPropertyChanged(nameof(LoginTooltip));

            SaveVM.SaveAppOptions();
            return true;
        }

        public bool? OpenGitPushWindow()
        {
            return WindowService?.OpenGitPushWindow();
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
                using Repository repo = new(CurrentRepository.Info.Path);
                GitOptions gitOpts = EditorVM.AppOptions.GitOptions;
                string password = gitOpts.UseToken
                    ? CredentialService.GetToken(gitOpts.UserName) ?? gitOpts.Password
                    : gitOpts.Password;
                // Credential information to fetch
                PullOptions options = new();
                options.FetchOptions = new FetchOptions();
                options.FetchOptions.CredentialsProvider = (_, _, _) =>
                    new UsernamePasswordCredentials()
                    {
                        Username = gitOpts.UserName,
                        Password = password
                    };

                // User information to create a merge commit
                Signature signature = new(gitOpts.UserName, gitOpts.Email, DateTimeOffset.Now);

                // Pull
                Commands.Pull(repo, signature, options);

                SetRepository(repo.Info.WorkingDirectory);
            }
            catch (Exception e)
            {
                Console.WriteLine(Resources.ErrorPullingRepositoryFormat, e.Message);
            }
        }

        private void PushRepository()
        {
            if (OpenGitPushWindow() != true)
            {
                return;
            }

            PushRepositorySolo();
        }

        private void PushRepositorySolo()
        {
            try
            {
                using Repository repo = new(CurrentRepository.Info.Path);
                
                GitOptions gitOpts = EditorVM.AppOptions.GitOptions;
                string password = gitOpts.UseToken
                    ? CredentialService.GetToken(gitOpts.UserName) ?? gitOpts.Password
                    : gitOpts.Password;

                Remote remote = repo.Network.Remotes[gitOpts.RemoteName];
                if (remote == null)
                {
                    if (!string.IsNullOrEmpty(gitOpts.RemoteRepositoryLink) &&
                        !gitOpts.RemoteRepositoryLink.Contains("<"))
                    {
                        Console.WriteLine(string.Format(Resources.RemoteOriginNotFoundAddingFormat, gitOpts.RemoteName, gitOpts.RemoteRepositoryLink));
                        remote = repo.Network.Remotes.Add(gitOpts.RemoteName, gitOpts.RemoteRepositoryLink);
                    }
                    else
                    {
                        Console.WriteLine(string.Format(Resources.RemoteOriginNotFoundNoLinkText, gitOpts.RemoteName));
                        return;
                    }
                }
                else if (!string.IsNullOrEmpty(gitOpts.RemoteRepositoryLink) && 
                         !gitOpts.RemoteRepositoryLink.Contains('<') && 
                         remote.Url != gitOpts.RemoteRepositoryLink)
                {
                    Console.WriteLine(Resources.GitUpdatingRemoteNameMessage, remote.Name, gitOpts.RemoteRepositoryLink);
                    repo.Network.Remotes.Update(remote.Name, r => r.Url = gitOpts.RemoteRepositoryLink);
                    remote = repo.Network.Remotes[gitOpts.RemoteName];
                }

                PushOptions options = new()
                {
                    CredentialsProvider = (_, _, _) =>
                    {
                        Console.WriteLine(Resources.GitProvidingCredentialsMessage, gitOpts.UserName);
                        return new UsernamePasswordCredentials()
                        {
                            Username = gitOpts.UserName,
                            Password = password
                        };
                    },
                    OnPushStatusError = (error) => 
                    {
                        Console.WriteLine(Resources.GitPushStatusErrorMessage, error.Reference, error.Message);
                    }
                };

                if (repo.Info.IsHeadDetached)
                {
                    string message = Resources.GitDetachedHeadErrorMessage;
                    MessageBox.Show(message);
                    throw new InvalidOperationException(message);
                }

                // Push the current branch to remote
                string pushRefSpec = $"{repo.Head.CanonicalName}:{repo.Head.CanonicalName}";
                string sha = repo.Head.Tip?.Sha ?? "no commits";
                BranchTrackingDetails tracking = repo.Head.TrackingDetails;
                string aheadStr = tracking != null ? (tracking.AheadBy?.ToString() ?? "0") : "unknown (no tracking)";


                CommitRepository();
                
                Console.WriteLine(Resources.GitPushingLocal, repo.Head.FriendlyName, sha, remote.Name, remote.Url);
                Console.WriteLine(Resources.GitPushBranchStateInfoMessage, pushRefSpec, aheadStr);
                repo.Network.Push(remote, [pushRefSpec], options);

                // Set upstream tracking if not set
                Branch localBranch = repo.Head;
                if (localBranch.TrackedBranch == null)
                {
                    repo.Branches.Update(localBranch, b => b.Remote = remote.Name, 
                        b => b.UpstreamBranch = localBranch.CanonicalName);
                    Console.WriteLine(Resources.GitSettingUpstreamBranch, localBranch.FriendlyName, remote.Name, localBranch.FriendlyName);
                }

                Console.WriteLine(Resources.GitRepositoryPushedMessage
                    , remote.Name);
                MessageBox.Show(string.Format(Resources.GitRepositoryPushedMessage
                    , remote.Name));
                IsRepositoryPushed = true;

                SetRepository(repo.Info.WorkingDirectory);
            }
            catch (Exception e)
            {
                IsRepositoryPushed = false;
                Console.WriteLine(Resources.GitPushingRepositoryErrorMessage, e.Message);
                MessageBox.Show(string.Format(Resources.GitPushingRepositoryErrorMessage, e.Message));
            }
        }

        private void CommitRepositoryWithSetup()
        {
            if (!OpenGitSetupWindow())
            {
                return;
            }

            CommitRepository();
        }

        private bool CommitRepository()
        {
            GitOptions options = EditorVM.AppOptions.GitOptions;
            
            Signature authorSignature = new(options.UserName, options.Email, DateTimeOffset.Now);
            
            try
            {
                using Repository repo = new(CurrentRepository.Info.Path);
                if (IsRepositoryUnstaged(repo))
                {
                    StageRepository(repo);
                    Console.WriteLine(Resources.GitRepositoryStagedMessage);
                }

                repo.Commit(options.CommitMessage, authorSignature, authorSignature);

                Console.WriteLine(Resources.GitRepositoryCommittedMessage);

                SetRepository(repo.Info.WorkingDirectory);

                return true;
            }
            catch (EmptyCommitException)
            {
                Console.WriteLine(Resources.NotCommitedError);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(Resources.UnexpectedErrorDuringCommitFormat, e.Message);
                return false;
            }
        }

        private bool StageRepository(Repository repo)
        {
            try
            {
                Commands.Stage(repo, "*");
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(Resources.ErrorStagingRepositoryFormat, repo.Info.WorkingDirectory, e.Message);
                return false;
            }
        }

        public void SetRepository(string path)
        {
            try
            {
                Console.WriteLine(Resources.SettingUpRepositoryText);
                CurrentRepository?.Dispose();
                CurrentRepository = new Repository(path);
                bool status = IsRepositoryUnstaged();
            }
            catch (RepositoryNotFoundException e)
            {
                Console.WriteLine(Resources.PathNotValidRepositoryFormat, path, e.Message);
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

                bool commited = CommitRepository();

                if (pushOnSave && commited)
                {
                    PushRepository();
                }
            }
        }

        public bool IsRepositoryUnstaged(Repository repo = null)
        {
            repo ??= CurrentRepository;
            return repo?.RetrieveStatus().IsDirty == true;
        }
        
        /// <summary>
        /// Gets the current git status of the repository as a string, concretely only changed files.
        /// </summary>
        /// <returns></returns>
        public static string GetGitStatus()
        {
            RepositoryStatus status = CurrentRepositoryStatic?.RetrieveStatus();
            
            return status == null 
                ? string.Format(Resources.GitStatusFailed) 
                : status.Modified.ToList().Count > 0 
                    ? CurrentRepositoryStatic.RetrieveStatus().Modified
                        .Select(e => e.FilePath)
                        .ToHumanReadableString()
                    : Resources.GitStatusNoChanges;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}