using CSVEditor.Model;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace CSVEditor.View.Controls
{
    /// <summary>
    /// Interaction logic for DirectoryWithCsvControl.xaml
    /// </summary>
    public partial class DirectoryWithCsvControl : UserControl, INotifyPropertyChanged
    {
        public DirectoryWithCsv DirectoryData
        {
            get { return (DirectoryWithCsv)GetValue(directoryProperty); }
            set { SetValue(directoryProperty, value); }
        }

        private static DirectoryWithCsv defaultRecord = new DirectoryWithCsv("Directory", new List<string> { "Files" });

        // Using a DependencyProperty as the backing store for Directory.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty directoryProperty =
            DependencyProperty.Register("DirectoryData", typeof(DirectoryWithCsv), typeof(DirectoryWithCsvControl), new PropertyMetadata(defaultRecord, SetRecord));

        public List<string> CsvFiles = new List<string>();

        private static void SetRecord(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            DirectoryWithCsvControl control = d as DirectoryWithCsvControl;

            if (control != null)
            {
                DirectoryWithCsv newValue = e.NewValue as DirectoryWithCsv;
                control.RootPathTextBlock.Text = newValue.DirectoryAbsolutePath;
                control.CsvFilesListBox.ItemsSource = newValue.CsvFilesNames;
            }
        }

        public string SelectedFile
        {
            get { return (string)GetValue(SelectedFileProperty); }
            set { SetValue(SelectedFileProperty, value); }
        }

        public static readonly DependencyProperty SelectedFileProperty =
            DependencyProperty.Register("SelectedFile", typeof(string), typeof(DirectoryWithCsvControl), new PropertyMetadata("SelectedFile"));

        public DirectoryWithCsvControl()
        {
            InitializeComponent();
        }

        private void list_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count != 0)
            {
                SelectedFile = e.AddedItems[0].ToString();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void CsvFilesListBox_LostFocus(object sender, RoutedEventArgs e)
        {
            ((ListBox)sender).SelectedItem = null;
        }
    }
}
