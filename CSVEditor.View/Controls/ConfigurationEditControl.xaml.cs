using CSVEditor.Model;
using CSVEditor.ViewModel;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace CSVEditor.View.Controls
{
    /// <summary>
    /// Interaction logic for ConfigurationEditControl.xaml
    /// </summary>
    public partial class ConfigurationEditControl : UserControl
    {
        private static readonly CsvFile DEFAULT_CSV_FILE = new CsvFile();

        private static EditGridControlViewModel VM;

        public Action GridUpdateRequestedEvent;

        public CsvFile CsvFile
        {
            get { return (CsvFile)GetValue(CsvFileProperty); }
            set { SetValue(CsvFileProperty, value); }
        }
        public static readonly DependencyProperty CsvFileProperty =
            DependencyProperty.Register("CsvFile", typeof(CsvFile), typeof(ConfigurationEditControl), new PropertyMetadata(DEFAULT_CSV_FILE, CsvFileChanged));

        public ConfigurationEditControl()
        {
            InitializeComponent();

            EditorVM.ConfiguraitonUpdated += RebuildGrid;
        }

        private static void CsvFileChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {           
            var control = (ConfigurationEditControl)d;
            var newValue = (CsvFile)e.NewValue;

            if (control == null || newValue == null)
            {
                return;
            }

            VM = new EditGridControlViewModel(
                control.Resources,
                control.DataContext as EditorVM,
                control.TopContainer);

            Console.WriteLine($"ConfigurationEditControl, new CsvFile set. Building new Grid for CsvFile: {Path.GetFileName((control.DataContext as EditorVM).SelectedCsvFile.AbsPath)}");

            control.TopContainer.Children.Clear();
            control.TopContainer.Children.Add(VM.GetEditConfigurationsGridForNewCsvFile());
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            VM.QueryForRelativePathToRootPathCommand.Execute();
        }

        private static void RebuildGrid(Grid topContainer)
        {
            topContainer.Children.Clear();
            topContainer.Children.Add(VM.GetEditConfigurationsGridForNewCsvFile());
            Console.WriteLine($"COnfigurationEditControl: RebuildGrid Executed.");
        }        
    }
}
