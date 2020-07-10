using CSVEditor.Model.HelperClasses;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using Prism.Commands;
using System;

namespace CSVEditor.ViewModel
{
    public class SaveVM : INotifyPropertyChanged
    {
        public string CsvFileText;

        public Window SaveWindow;

        private string csvFilePath;
        public string CsvFilePath
        {
            get { return csvFilePath; }
            set 
            {
                csvFilePath = value;
                OnPropertyChanged();
            }
        }

        private SaveOptions saveOptions;    
        public SaveOptions SaveOptions
        {
            get { return saveOptions; }
            set 
            {
                saveOptions = value;
                OnPropertyChanged();
            }
        }

        public DelegateCommand OverwriteCommand { get; set; }
        public DelegateCommand SaveAsCommand { get; set; }
        public DelegateCommand SaveAltrnativePathCommand { get; set; }
        public DelegateCommand CancelCommand { get; set; }

        public SaveVM()
        {
            OverwriteCommand = new DelegateCommand(Overwrite);
            SaveAsCommand = new DelegateCommand(SaveAs);
            SaveAltrnativePathCommand = new DelegateCommand(SaveAlternativePath);
            CancelCommand = new DelegateCommand(Cancel);
        }

        private void Cancel()
        {
            SaveWindow?.Close();
        }

        private void SaveAlternativePath()
        {
            throw new NotImplementedException();
        }

        private void SaveAs()
        {
            throw new NotImplementedException();
        }

        private void Overwrite()
        {
            throw new NotImplementedException();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
