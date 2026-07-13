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
            IsAuthenticated = false;
            RemoteRepositoryLink = "<Remote Repository Link>";
            RemoteName = DefaultRemoteName;
        }
        
        /// <summary>
        /// Creates a new instance of GitOptions by copying the values from the provided source
        /// instance (DeppCopy unless somo object is added later).
        /// If the source is null, it initializes the properties with default values.
        /// </summary>
        /// <param name="source"></param>
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

        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
