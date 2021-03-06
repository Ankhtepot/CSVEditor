﻿using CSVEditor.Model.Services;
using CSVEditor.View.Controls;
using CSVEditor.ViewModel;
using System.Windows;
using System.Windows.Controls;

namespace CSVEditor.View
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class EditorWindow : Window
    {
        public EditorVM EditorVM;

        public static LineEditControl lineEditControl;

        public delegate void SetMainTabSelectedTabIndexEvent(int tabIndex);
        public EditorWindow()
        {
            InitializeComponent();
            EditorVM = new EditorVM(new WindowService());
            EditorVM.RequestChangeTab += SetMainTabSelectedTabIndex;
            TopContainer.DataContext = EditorVM;

            Closing += EditorVM.OnWindowClosing;

            CsvFileGridViewer.SetMainTabSelectedTabIndex(SetMainTabSelectedTabIndex);
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RebuildCsvFileGridViewer();
        }

        public void SetMainTabSelectedTabIndex(int TabIndex)
        {
            MainTabControl.SelectedIndex = TabIndex.Clamp(0, MainTabControl.Items.Count - 1);
        }

        private void RebuildCsvFileGridViewer()
        {
            if (MainTabControl.SelectedIndex == 0) // means, OverView Tab of MainTabControl is selected
            {                
                CsvFileGridViewerControl.BuildGrid(CsvFileGridViewer, EditorVM.SelectedCsvFile, LineEditorWrapper);
            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            new ReplaceImageWindow().ShowDialog();
        }
    }
}
