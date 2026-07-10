using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace CSVEditor.Core.HelperClasses
{
    public class DirectoryWithCsv : INotifyPropertyChanged
    {
        public string DirectoryAbsolutePath
        {
            get;
            set
            {
                field = value;
                OnPropertyChanged();
            }
        }

        public List<string> CsvFilesNames { get; set; }

        public DirectoryWithCsv() : this ("", []) {}

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
