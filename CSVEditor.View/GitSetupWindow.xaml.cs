using CSVEditor.Core.HelperClasses;
using CSVEditor.Core.Services;
using Octokit;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace CSVEditor.View
{
    /// <summary>
    /// Interaction logic for GitSetupWindow.xaml
    /// </summary>
    public partial class GitSetupWindow : Window, INotifyPropertyChanged
    {
        private GitOptions gitOptions;
        private string _lastCheckedUsername;

        public GitOptions GitOptions
        {
            get { return gitOptions; }
            set 
            {
                gitOptions = value;
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
                MessageBox.Show("Please enter a Personal Access Token first.");
                return;
            }

            try
            {
                var client = new GitHubClient(new ProductHeaderValue("CSVEditor"));
                var tokenAuth = new Credentials(token);
                client.Credentials = tokenAuth;

                var user = await client.User.Current();

                GitOptions.UserName = user.Login;
                if (!string.IsNullOrEmpty(user.Email))
                {
                    GitOptions.Email = user.Email;
                }
                GitOptions.IsAuthenticated = true;
                
                string msg = $"Successfully authenticated as {user.Login}!";
                if (string.IsNullOrEmpty(user.Email))
                {
                    msg += "\n\nNote: Your GitHub email is private and couldn't be fetched. You can manually enter the email you want to use for commit signatures.";
                }
                MessageBox.Show(msg);
            }
            catch (Exception ex)
            {
                GitOptions.IsAuthenticated = false;
                string message = ex.Message;
                if (ex is AuthorizationException)
                {
                    message += "\n\nTip: For GitHub, you must use a Personal Access Token (PAT). Regular passwords are no longer supported for API access.";
                }
                MessageBox.Show($"Authentication failed: {message}");
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
            Close();
        }

        private void UserNameTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (GitOptions.UseToken && GitOptions.UserName != _lastCheckedUsername)
            {
                _lastCheckedUsername = GitOptions.UserName;
                var token = CredentialService.GetToken(GitOptions.UserName);
                if (!string.IsNullOrEmpty(token))
                {
                    PasswordBox.Password = token;
                }
            }
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
