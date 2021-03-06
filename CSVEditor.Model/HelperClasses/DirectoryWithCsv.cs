﻿using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace CSVEditor.Model.HelperClasses
{
    public class DirectoryWithCsv : INotifyPropertyChanged
    {
        private string directoryAbsolutePath;
        public string DirectoryAbsolutePath {
            get => directoryAbsolutePath;
            set { directoryAbsolutePath = value; OnPropertyChanged(nameof(DirectoryAbsolutePath)); }
        }

        private List<string> csvFilesNames;
        public List<string> CsvFilesNames
        {
            get => csvFilesNames;
            set => csvFilesNames = value;
        }

        public DirectoryWithCsv() : this ("", new List<string>()) {}

        public DirectoryWithCsv(string directoryPath, List<string> fileNames)
        {
            DirectoryAbsolutePath = directoryPath;
            CsvFilesNames = fileNames;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
