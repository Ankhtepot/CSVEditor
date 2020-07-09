using CSVEditor.Model.HelperClasses;
using CSVEditor.ViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace CSVEditor.View
{
    /// <summary>
    /// Interaction logic for SaveWindow.xaml
    /// </summary>
    public partial class SaveWindow : Window, INotifyPropertyChanged
    {
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

        public string RootRepositoryPath { get; set; }

        public SaveWindow(SaveOptions saveOptions, string rootRepositoryPath)
        {
            InitializeComponent();
            SaveOptions = saveOptions;
            RootRepositoryPath = rootRepositoryPath;
            DataContext = this;
            //(DataContext as SaveVM).SaveOptions = saveOptions;
            
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
