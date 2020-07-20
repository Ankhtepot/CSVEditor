using System;
using System.Collections.Generic;
using System.Text;

namespace CSVEditor.Model.HelperClasses
{
    public class GitOptions
    {
        public static string DefaultCommitMessage = "<change in CSV file>";
        public static string DefaultRemoteBranch = "origin";

        public string CommitMessage { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string RemoteRepositoryLink { get; set; }
        public string RemoteBranch { get; set; }

        public GitOptions()
        {
            CommitMessage = DefaultCommitMessage;
            UserName = "<UserName>";
            Email = "<Email>";
            Password = "";
            RemoteRepositoryLink = "<Remote Repository Link>";
            RemoteBranch = DefaultRemoteBranch;
        }
    }
}
