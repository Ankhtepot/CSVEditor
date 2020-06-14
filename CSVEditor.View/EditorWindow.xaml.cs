using CSVEditor.View.Controls;
using CSVEditor.ViewModel;
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
        public EditorWindow()
        {
            InitializeComponent();
            EditorVM = new EditorVM();
            TopContainer.DataContext = EditorVM;
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //TODO: Find better solution for updating CsvFileGridViewer and trigger it only after changing something in LineEditor
            var control = (TabControl)sender;
            if (control.SelectedIndex == 0)
            {
                var binding = new Binding("SelectedCsvFile");
                binding.Source = EditorVM;
                CsvFileGridVIewer.InputCsvFile = null;
                CsvFileGridVIewer.InputCsvFile = EditorVM.SelectedCsvFile;
                BindingOperations.SetBinding(CsvFileGridVIewer, CsvFileGridViewerControl.InputCsvFileProperty, binding);
            }
        }
    }
}
