using CSVEditor.Core.HelperClasses;
using CSVEditor.Core.Services;
using Octokit;
using System;
using System.Threading.Tasks;
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

        private readonly GitOptions _gitOptionsOnOpen;
        
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
            
            _gitOptionsOnOpen = new GitOptions(GitOptions);
        }

        private void GitHubLoginButton_Click(object sender, RoutedEventArgs e) =>
            _ = AsyncService.RunAsync(async () => await ProcessGitHubLoginButton_Click_Async());

        private async Task ProcessGitHubLoginButton_Click_Async(
            string userNameOverride = null,
            string tokenOverride = null,
            bool forceIsAuthenticatedOnChange = false
            )
        {
            string enteredUserName = string.IsNullOrEmpty(userNameOverride) ? NameTextBox.Text.Trim() : userNameOverride;
            string token = string.IsNullOrEmpty(tokenOverride) ? PasswordBox.Password : tokenOverride;

            if (AreLoginEntriesFalsy(enteredUserName, token))
                return;

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

                ProcessLoginSuccess(user, token, enteredUserName, forceIsAuthenticatedOnChange);
            }
            catch (Exception ex)
            {
                ProcessLogInFailure(ex);
            }
        }

        private static bool AreLoginEntriesFalsy(string enteredUserName, string token)
        {
            string errorMessage = T.FixErrorsPrompt + Environment.NewLine + Environment.NewLine;
            bool hasErrors = false;

            if (string.IsNullOrEmpty(enteredUserName))
            {
                errorMessage += T.PleaseEnterUserNamePrompt + "\n";
                hasErrors = true;
            }

            if (string.IsNullOrEmpty(token))
            {
                errorMessage += T.EnterPATPrompt;
                hasErrors = true;
            }

            if (!hasErrors)
            {
                return false;
            }

            MessageBoxHelper.ShowProcessErrorBox(T.GitLoginEntriesFalsy, errorMessage);
            return true;
        }

        private void ProcessLoginSuccess(
            User user,
            string token,
            string enteredUserName,
            bool forceIsAuthenticatedOnChange = false
            )
        {
            if (!string.IsNullOrEmpty(user.Email))
            {
                GitOptions.Email = user.Email;
            }
            GitOptions.Password = token;
            GitOptions.UserName = enteredUserName;
            GitOptions.IsAuthenticated = true;
            
            if (forceIsAuthenticatedOnChange)
            {
                AppOptionsService.AppOptions.GitOptions.OnPropertyChanged(nameof(GitOptions.IsAuthenticated));
            }

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

        private void ProcessLogInFailure(Exception ex)
        {
            AppOptionsService.AppOptions.GitOptions.IsAuthenticated = false;
            _validatedToken = null;

            string message = ex.Message;
            if (ex is AuthorizationException)
            {
                message += $"\n\n{T.GitPATTipText}";
            }

            MessageBox.Show(string.Format(T.GitAuthFailed, message));
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e) =>
            _ = AsyncService.RunAsync(async () => await ProcessCancelButton_Click_Async());

        private async Task ProcessCancelButton_Click_Async()
        {
            // Revert conditions:
            // If user changed login and is authenticated with token -> relog to previous user, restore the rest
            // If user changed login or not but is not authenticated with token anymore -> relog previous user, restore all previous values
            // If user did not change the login -> just restore all previous values
            bool userChangedLogin = !string.Equals(GitOptions.UserName, _gitOptionsOnOpen.UserName, StringComparison.OrdinalIgnoreCase);
            bool switchedToAnotherAuthenticatedUser = userChangedLogin && GitOptions.IsAuthenticated;
            bool userLoggedOffAuthenticatedUser = _gitOptionsOnOpen.IsAuthenticated && !GitOptions.IsAuthenticated;
            
            RestoreGitOptionsToOnOpenState();
            if (userLoggedOffAuthenticatedUser || switchedToAnotherAuthenticatedUser)
            {
                await ProcessGitHubLoginButton_Click_Async(GitOptions.UserName, GitOptions.Password, true);
            }

            Close();
            return;

            void RestoreGitOptionsToOnOpenState() => AppOptionsService.SetGitOptions(_gitOptionsOnOpen);
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
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

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
            e.Handled = true;
        }
    }
}
