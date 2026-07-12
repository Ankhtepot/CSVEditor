using System.Text.Json.Serialization;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace CSVEditor.Core.HelperClasses
{
    public class GitOptions : INotifyPropertyChanged
    {
        private const string DefaultCommitMessage = "<change in CSV file>";
        private const string DefaultRemoteName = "origin";

        public string CommitMessage
        {
            get;
            set
            {
                if (value == field)
                    return;
                
                field = value;
                OnPropertyChanged();
            }
        }

        public string UserName
        {
            get;
            set
            {
                if (value == field)
                    return;
                
                field = value;
                OnPropertyChanged();
            }
        }

        public string Email
        {
            get;
            set
            {
                if (value == field)
                    return;
                
                field = value;
                OnPropertyChanged();
            }
        }

        [JsonIgnore]
        public string Password { get; set; }

        public bool UseToken
        {
            get;
            set
            {
                if (value == field)
                    return;
                
                field = value;
                OnPropertyChanged();
            }
        }

        public bool IsAuthenticated
        {
            get;
            set
            {
                if (value == field)
                    return;
                
                field = value;
                OnPropertyChanged();
            }
        }

        public string RemoteRepositoryLink
        {
            get;
            set
            {
                if (value == field)
                    return;
                
                field = value;
                OnPropertyChanged();
            }
        }

        public string RemoteName
        {
            get;
            set
            {
                if (value == field)
                    return;
                
                field = value;
                OnPropertyChanged();
            }
        }

        public GitOptions()
        {
            CommitMessage = DefaultCommitMessage;
            UserName = "<UserName>";
            Email = "<Email>";
            Password = "";
            UseToken = true;
            RemoteRepositoryLink = "<Remote Repository Link>";
            RemoteName = DefaultRemoteName;
        }

        public GitOptions(GitOptions source)
        {
            if (source == null)
            {
                CommitMessage = DefaultCommitMessage;
                UserName = "<UserName>";
                Email = "<Email>";
                Password = "";
                UseToken = true;
                IsAuthenticated = false;
                RemoteRepositoryLink = "<Remote Repository Link>";
                RemoteName = DefaultRemoteName;
                return;
            }

            CommitMessage = source.CommitMessage;
            UserName = source.UserName;
            Email = source.Email;
            Password = source.Password;
            UseToken = source.UseToken;
            IsAuthenticated = source.IsAuthenticated;
            RemoteRepositoryLink = source.RemoteRepositoryLink;
            RemoteName = source.RemoteName;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
