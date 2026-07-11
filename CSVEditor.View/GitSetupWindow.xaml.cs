using CSVEditor.Core.HelperClasses;
using CSVEditor.Core.Services;
using Octokit;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using EventManager = CSVEditor.Core.Services.EventManager;
using T = CSVEditor.Core.Properties.Resources;

namespace CSVEditor.View
{
    /// <summary>
    /// Interaction logic for GitSetupWindow.xaml
    /// </summary>
    public partial class GitSetupWindow : Window, INotifyPropertyChanged
    {
        private string _lastCheckedUsername;
        private string _validatedToken;
        private GitOptions _gitOptions;

        public GitOptions GitOptions
        {
            get => _gitOptions;
            set
            {
                _gitOptions = value;
                OnPropertyChanged();
            }
        }

        public bool Canceled { get; set; }

        // public GitSetupWindow(GitOptions gitOptions)
        public GitSetupWindow(GitOptions gitOptions)
        {
            InitializeComponent();

            GitOptions = gitOptions;
            DataContext = this;
            Canceled = true;
            _lastCheckedUsername = null;

            if (GitOptions.UseToken)
            {
                if (GitOptions.IsAuthenticated)
                {
                    _validatedToken = CredentialService.GetToken(GitOptions.UserName);
                    PasswordBox.Password = _validatedToken ?? "";
                    _lastCheckedUsername = GitOptions.UserName;
                }
                else
                {
                    FillPasswordForCurrentUser(force: true);
                }
            }
            else
            {
                PasswordBox.Password = GitOptions.Password;
            }
        }

        private async void GitHubLoginButton_Click(object sender, RoutedEventArgs e)
        {
            string enteredUserName = GitOptions.UserName?.Trim();
            string token = PasswordBox.Password;
            GitOptions.UserName = enteredUserName;

            if (string.IsNullOrEmpty(enteredUserName))
            {
                MessageBox.Show(T.PleaseEnterUserNamePrompt);
                return;
            }

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

                if (!string.Equals(enteredUserName, user.Login, StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException(T.GitTokenNotMatchingUser);
                }

                if (!string.IsNullOrEmpty(user.Email))
                {
                    GitOptions.Email = user.Email;
                }
                GitOptions.Password = token;
                GitOptions.IsAuthenticated = true;
                if (GitOptions.UseToken)
                {
                    CredentialService.SaveToken(GitOptions.UserName, token);
                }
                _validatedToken = token;
                _lastCheckedUsername = GitOptions.UserName;

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
                GitOptions.Password = null;
                _validatedToken = null;

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
            EventManager.TriggerGitOptionsWindowClosed();
            Close();
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            if (GitOptions.UseToken)
            {
                if (!GitOptions.IsAuthenticated)
                {
                    GitOptions.Password = null;
                }
            }
            else
            {
                GitOptions.Password = PasswordBox.Password;
            }

            AppOptionsService.SaveAppOptions();

            Canceled = false;

            EventManager.TriggerGitOptionsWindowClosed();

            Close();
        }

        private void UserNameTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            FillPasswordForCurrentUser(force: true);
        }

        private void UserNameTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                FillPasswordForCurrentUser(force: true);
                e.Handled = true;
            }
        }

        private void FillPasswordForCurrentUser(bool force = false)
        {
            if (!GitOptions.UseToken)
                return;

            string username = GitOptions.UserName?.Trim() ?? "";

            if (GitOptions.UserName != username)
            {
                GitOptions.UserName = username;
            }

            if (!force &&
                string.Equals(username,
                    _lastCheckedUsername,
                    StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            _lastCheckedUsername = username;

            MigrateLegacyToken(username);

            string token = CredentialService.GetToken(username);

            if (!string.IsNullOrEmpty(token))
            {
                PasswordBox.Password = token;
                GitOptions.Password = token;
                
                // If it's the same token as already validated, keep authenticated status
                if (token != _validatedToken)
                {
                    GitOptions.IsAuthenticated = false;
                    _validatedToken = null;
                }
            }
            else
            {
                PasswordBox.Password = "";
                GitOptions.Password = "";
                GitOptions.IsAuthenticated = false;
                _validatedToken = null;
            }
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            string password = PasswordBox.Password;
            GitOptions.Password = password;

            if (GitOptions.IsAuthenticated && password != _validatedToken)
            {
                GitOptions.IsAuthenticated = false;
                _validatedToken = null;
            }
        }
        
        private void MigrateLegacyToken(string username)
        {
            string token = CredentialService.GetToken(username);

            if (!string.IsNullOrEmpty(token))
                return;

            string legacy = CredentialService.GetLegacyToken();

            if (string.IsNullOrWhiteSpace(legacy))
                return;

            CredentialService.SaveToken(username, legacy);
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
