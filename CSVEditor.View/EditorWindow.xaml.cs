using CSVEditor.Model.Services;
using CSVEditor.View.Controls;
using CSVEditor.ViewModel;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace CSVEditor.View
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class EditorWindow : Window
    {
        public EditorVM EditorVM;

        public delegate void SetMainTabSelectedTabIndexEvent(int tabIndex);
        public delegate void RebuildConfigurationGridEvent();
        public EditorWindow()
        {
            InitializeComponent();
            EditorVM = new EditorVM();
            TopContainer.DataContext = EditorVM;

            CsvFileGridVIewer.SetMainTabSelectedTabIndex(SetMainTabSelectedTabIndex);
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //TODO: Find better solution for updating CsvFileGridViewer and trigger it only after changing something in LineEditor
            var control = (TabControl)sender;
            if (control.SelectedIndex == 0)
            {
                CsvFileGridVIewer.InputCsvFile = null;
                BindingOperations.SetBinding(CsvFileGridVIewer,
                    CsvFileGridViewerControl.InputCsvFileProperty,
                    new Binding("SelectedCsvFile") { Source = EditorVM});
            }
        }

        public void SetMainTabSelectedTabIndex(int TabIndex)
        {
            MainTabControl.SelectedIndex = TabIndex.Clamp(0, MainTabControl.Items.Count - 1);
        }

        public void RebuildConfigurationGrid()
        {
            ConfigurationEditControl.RebuildGrid(ConfigurationEditGrid.TopContainer);
        }
    }
}
