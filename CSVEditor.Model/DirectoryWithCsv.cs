using System.Collections.ObjectModel;
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


        public ObservableCollection<string> CsvFilesNames { get; set; }

        public DirectoryWithCsv(string directoryPath, ObservableCollection<string> fileNames)
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
