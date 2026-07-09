using System.Text.Json.Serialization;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace CSVEditor.Core.HelperClasses
{
    public class GitOptions : INotifyPropertyChanged
    {
        private const string DefaultCommitMessage = "<change in CSV file>";
        private const string DefaultRemoteBranch = "origin";

        private string commitMessage;
        public string CommitMessage
        {
            get => commitMessage;
            set { commitMessage = value; OnPropertyChanged(); }
        }

        private string userName;
        public string UserName
        {
            get => userName;
            set { userName = value; OnPropertyChanged(); }
        }

        private string email;
        public string Email
        {
            get => email;
            set { email = value; OnPropertyChanged(); }
        }
        
        [JsonIgnore]
        public string Password { get; set; }

        private bool useToken;
        public bool UseToken
        {
            get => useToken;
            set { useToken = value; OnPropertyChanged(); }
        }

        private bool isAuthenticated;
        public bool IsAuthenticated
        {
            get => isAuthenticated;
            set { isAuthenticated = value; OnPropertyChanged(); }
        }

        private string remoteRepositoryLink;
        public string RemoteRepositoryLink
        {
            get => remoteRepositoryLink;
            set { remoteRepositoryLink = value; OnPropertyChanged(); }
        }

        private string remoteBranch;
        public string RemoteBranch
        {
            get => remoteBranch;
            set { remoteBranch = value; OnPropertyChanged(); }
        }

        public GitOptions()
        {
            CommitMessage = DefaultCommitMessage;
            UserName = "<UserName>";
            Email = "<Email>";
            Password = "";
            UseToken = true;
            RemoteRepositoryLink = "<Remote Repository Link>";
            RemoteBranch = DefaultRemoteBranch;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
