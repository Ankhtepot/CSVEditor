using CSVEditor.Core.Interfaces;
using Prism.Commands;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using CSVEditor.Core.Services;
using CSVEditor.Core.HelperClasses;
using CSVEditor.Core.Properties;
using EventManager = CSVEditor.Core.Services.EventManager;

namespace CSVEditor.ViewModel
{
    //*************************************************************************
    //************************    testing repo info     ***********************
    //git remote add origin https://github.com/Ankhtepot/CsvEditorTesting.git
    //git to kocourweb: https://github.com/miljed/kocourweb.git
    //git push -u origin master
    //*************************************************************************

    public class GitVM : INotifyPropertyChanged, IDisposable
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

        public bool IsLoggedIn 
        {
            get => field;
            set
            {
                field = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(LoginTooltip));
            }
        }

        public string LoginTooltip 
        {
            get => field;
            set
            {
                field = value;
                OnPropertyChanged();
            }
        }

        public DelegateCommand OpenGitSetupCommand { get; set; }
        public DelegateCommand CommitRepositoryCommand { get; set; }
        public static DelegateCommand PushRepositoryCommand { get; set; }
        public DelegateCommand PullRepositoryCommand { get; set; }
        
        private bool _disposed;

        public GitVM(EditorVM EditorVM)
        {
            WindowService = EditorVM.WindowService;
            
            OpenGitSetupCommand = new DelegateCommand(OpenGitSetup);
            CommitRepositoryCommand = new DelegateCommand(CommitRepositoryWithSetup);
            PushRepositoryCommand = new DelegateCommand(PushRepository);
            PullRepositoryCommand = new DelegateCommand(PullRepository);

            EventManager.OnGitOptionsWindowRequested += OpenGitSetup;
            SaveVM.OnSaved += ProcessRepositoryOnSave;

            SubscribeToGitOptions();
        }

        public async Task InitializeAsync()
        {
            if (AppOptionsService.AppOptions?.GitOptions?.IsAuthenticated == true)
            {
                await GitService.VerifyGitHubLoginAsync();

                SetLoginProperties();
            }
        }

        private void SetLoginProperties()
        {
            IsLoggedIn = EditorVM.AppOptions?.GitOptions?.IsAuthenticated ?? false;
            LoginTooltip = IsLoggedIn
                ? string.Format(Resources.LoggedInAsFormat, EditorVM.AppOptions?.GitOptions?.UserName,
                    EditorVM.AppOptions?.GitOptions?.Email)
                : Resources.LogInHelpText;
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
                SetLoginProperties();
            }
        }

        public void SetGitInfo(string rootRepositoryPath)
        {
            SubscribeToGitOptions();
            IsGitRepo = FileSystemService.IsDirectoryWithGitRepository(rootRepositoryPath);
            if (IsGitRepo)
            {
                GitService.SetRepository(rootRepositoryPath);
            }
        }

        private void OpenGitSetup()
        {
            OpenGitSetupWindow();
        }

        private bool OpenGitSetupWindow()
        {
            SubscribeToGitOptions();

            bool accepted = WindowService?.OpenGitSetupWindow() ?? false;

            if (!accepted)
                return false;

            AppOptionsService.SaveAppOptions();

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

            PullRepositoryDirect();
        }

        private void PullRepositoryDirect()
        {
            try
            {
                GitService.Pull(EditorVM.AppOptions.GitOptions);
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

            PushRepositoryDirect();
        }

        private void PushRepositoryDirect()
        {
            try
            {
                GitOptions gitOpts = EditorVM.AppOptions.GitOptions;
                GitService.Push(gitOpts);
                MessageBox.Show(string.Format(Resources.GitRepositoryPushedMessage, gitOpts.RemoteName));
                IsRepositoryPushed = true;
            }
            catch (InvalidOperationException e)
            {
                IsRepositoryPushed = false;
                Console.WriteLine(Resources.GitPushingRepositoryErrorMessage, e.Message);
                MessageBox.Show(e.Message);
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

        private bool CommitRepository() => GitService.CommitRepository(EditorVM.AppOptions.GitOptions);

        private void ProcessRepositoryOnSave(bool commitOnSave, bool pushOnSave)
        {
            if (!commitOnSave) return;

            bool commited = CommitRepository();

            if (pushOnSave && commited)
            {
                PushRepository();
            }
        }

        /// <summary>
        /// Gets the current git status of the repository as a string, concretely only changed files.
        /// </summary>
        public static string GetGitStatus() => GitService.GetGitStatus();

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void Dispose()
        {
            if (_disposed)
                return;
            
            EventManager.OnGitOptionsWindowRequested -= OpenGitSetup;
            SaveVM.OnSaved -= ProcessRepositoryOnSave;

            if (EditorVM.AppOptions?.GitOptions != null)
            {
                EditorVM.AppOptions.GitOptions.PropertyChanged -= GitOptions_PropertyChanged;
            }
            
            _disposed = true;
            GC.SuppressFinalize(this);
        }
    }
}