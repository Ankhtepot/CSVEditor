using System.Collections.Generic;
using System.ComponentModel;

namespace CSVEditor.Model
{
    public class DirectoryWithCsv : INotifyPropertyChanged
    {
        private string directoryAbsolutePath;
        public string DirectoryAbsolutePath {
            get { return directoryAbsolutePath; }
            set { directoryAbsolutePath = value; OnPropertyChanged(nameof(DirectoryAbsolutePath)); }
        }

        private List<string> csvFilesNames;
        public List<string> CsvFilesNames
        {
            get { return csvFilesNames; }
            set { csvFilesNames = value; }
        }

        public DirectoryWithCsv() : this ("", new List<string>())
        {
        }

        public DirectoryWithCsv(string directoryPath, List<string> fileNames)
        {
            DirectoryAbsolutePath = directoryPath;
            CsvFilesNames = fileNames;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
