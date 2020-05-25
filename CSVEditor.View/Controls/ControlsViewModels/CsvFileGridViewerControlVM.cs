using CSVEditor.Model;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace CSVEditor.View.Controls.ControlsViewModels
{
    public class CsvFileGridViewerControlVM : INotifyPropertyChanged
    {
        public static readonly CsvFile DEFAULT_FILE = new CsvFile();

        private CsvFile csvFile;

        public CsvFile LocalCsvFile
        {
            get { return csvFile; }
            set { csvFile = value; OnPropertyChanged(); }
        }

        public CsvFileGridViewerControlVM()
        {
            LocalCsvFile = DEFAULT_FILE;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
