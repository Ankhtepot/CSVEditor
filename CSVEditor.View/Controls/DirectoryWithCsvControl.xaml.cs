﻿using CSVEditor.Model.HelperClasses;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace CSVEditor.View.Controls
{
    /// <summary>
    /// Interaction logic for DirectoryWithCsvControl.xaml
    /// </summary>
    public partial class DirectoryWithCsvControl : UserControl, INotifyPropertyChanged
    {
        private static readonly DirectoryWithCsv DefaultRecord = new DirectoryWithCsv("Directory", new List<string> { "Files" });

        public List<string> CsvFiles = new List<string>();

        public DirectoryWithCsv DirectoryData
        {
            get { return (DirectoryWithCsv)GetValue(directoryProperty); }
            set { SetValue(directoryProperty, value); }
        }
        public static readonly DependencyProperty directoryProperty =
            DependencyProperty.Register("DirectoryData", typeof(DirectoryWithCsv), typeof(DirectoryWithCsvControl), new PropertyMetadata(DefaultRecord, DirectoryDataChanged));

        private static void DirectoryDataChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
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
