using CSVEditor.Core.HelperClasses;
using CSVEditor.Core.Services;
using Octokit;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using T = CSVEditor.Core.Properties.Resources;

namespace CSVEditor.View
{
    /// <summary>
    /// Interaction logic for GitSetupWindow.xaml
    /// </summary>
    public partial class GitSetupWindow : Window, INotifyPropertyChanged
    {
        private string _lastCheckedUsername;

        public GitOptions GitOptions
        {
            get;
            set
            {
                field = value;
                OnPropertyChanged();
            }
        }

        public bool Canceled { get; set; }

        public GitSetupWindow(GitOptions gitOptions)
        {
            InitializeComponent();

            DataContext = this;
            Canceled = true;
            GitOptions = gitOptions;
            _lastCheckedUsername = GitOptions.UserName;

            if (GitOptions.UseToken)
            {
                PasswordBox.Password = CredentialService.GetToken(GitOptions.UserName) ?? GitOptions.Password;
            }
            else
            {
                PasswordBox.Password = GitOptions.Password;
            }
        }

        private async void GitHubLoginButton_Click(object sender, RoutedEventArgs e)
        {
            string token = PasswordBox.Password;
            if (string.IsNullOrEmpty(token))
            {
                MessageBox.Show(T.EnterPATPrompt);
                return;
            }

            try
            {
                GitHubClient client = new(new ProductHeaderValue("CSVEditor"));
                Credentials tokenAuth = new(token);
                client.Credentials = tokenAuth;

                User user = await client.User.Current();

                GitOptions.UserName = user.Login;
                if (!string.IsNullOrEmpty(user.Email))
                {
                    GitOptions.Email = user.Email;
                }
                GitOptions.IsAuthenticated = true;

                string msg = string.Format(T.GitAuthSuccessText, user.Login);
                if (string.IsNullOrEmpty(user.Email))
                {
                    msg += $"\n\n{T.GitCouldntFetchEmailText}";
                }
                MessageBox.Show(msg);
            }
            catch (Exception ex)
            {
                GitOptions.IsAuthenticated = false;
                string message = ex.Message;
                if (ex is AuthorizationException)
                {
                    message += $"\n\n{T.GitPATTipText}";
                }

                MessageBox.Show(string.Format(T.GitAuthFailed, message));
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            GitOptions.Password = PasswordBox.Password;
            if (GitOptions.UseToken)
            {
                CredentialService.SaveToken(GitOptions.UserName, GitOptions.Password);
            }

            Canceled = false;
            // SaveGitOptions();
            Close();
        }

        private void UserNameTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (GitOptions.UseToken && GitOptions.UserName != _lastCheckedUsername)
            {
                _lastCheckedUsername = GitOptions.UserName;
                string token = CredentialService.GetToken(GitOptions.UserName);
                if (!string.IsNullOrEmpty(token))
                {
                    PasswordBox.Password = token;
                }
            }
        }

        private void SaveGitOptions()
        {
            AppOptionsService.AppOptions.GitOptions = GitOptions;
            AppOptionsService.SaveAppOptions();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
            e.Handled = true;
        }
    }
}
