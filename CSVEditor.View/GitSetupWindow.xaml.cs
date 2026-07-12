using CSVEditor.Core.HelperClasses;
using CSVEditor.Core.Services;
using Octokit;
using System;
using System.Windows;
using System.Windows.Input;
using T = CSVEditor.Core.Properties.Resources;

namespace CSVEditor.View
{
    /// <summary>
    /// Interaction logic for GitSetupWindow.xaml
    /// </summary>
    public partial class GitSetupWindow : Window
    {
        private string _validatedToken;
        
        public GitOptions GitOptions => AppOptionsService.AppOptions.GitOptions;

        public bool Canceled { get; set; }

        // public GitSetupWindow(GitOptions gitOptions)
        public GitSetupWindow()
        {
            InitializeComponent();
            
            DataContext = this;
            Canceled = true;

            if (GitOptions.UseToken)
            {
                if (GitOptions.IsAuthenticated)
                {
                    _validatedToken = CredentialService.GetToken(GitOptions.UserName);
                    PasswordBox.Password = _validatedToken ?? "";
                    NameTextBox.Text = GitOptions.UserName;
                }
                else
                {
                    FillPasswordForCurrentUser();
                }
            }
            else
            {
                PasswordBox.Password = GitOptions.Password;
            }
            
            NameTextBox.Text = GitOptions.UserName;
        }

        private async void GitHubLoginButton_Click(object sender, RoutedEventArgs e)
        {
            string enteredUserName = NameTextBox.Text.Trim();
            string token = PasswordBox.Password;

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
                GitOptions.UserName = enteredUserName;
                GitOptions.IsAuthenticated = true;
                if (GitOptions.UseToken)
                {
                    CredentialService.SaveToken(GitOptions.UserName, token);
                }
                _validatedToken = token;

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
            
            GitOptions.UserName = NameTextBox.Text.Trim();

            AppOptionsService.SaveAppOptions();

            Canceled = false;

            Close();
        }

        private void UserNameTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            FillPasswordForCurrentUser();
        }

        private void UserNameTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                FillPasswordForCurrentUser();
                e.Handled = true;
            }
        }

        private void FillPasswordForCurrentUser()
        {
            if (!GitOptions.UseToken)
                return;

            string username = NameTextBox.Text.Trim();

            string token = CredentialService.GetToken(username);

            if (!string.IsNullOrEmpty(token))
            {
                PasswordBox.Password = token;
                
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

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
            e.Handled = true;
        }
    }
}
