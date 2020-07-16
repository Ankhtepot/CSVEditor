using System;
using System.Collections.Generic;
using System.Text;

namespace CSVEditor.Model.HelperClasses
{
    public class GitOptions
    {
        public static string DefaultCommitMessage = "<change in CSV file>";

        public string CommitMessage { get; set; }
        public string Author { get; set; }
        public string Email { get; set; }

        public GitOptions()
        {
            CommitMessage = DefaultCommitMessage;
            Author = "<Author>";
            Email = "<Email>";
        }
    }
}
