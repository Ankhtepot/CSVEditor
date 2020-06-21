using CSVEditor.Model;
using CSVEditor.ViewModel;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CSVEditor.View.Controls
{
    /// <summary>
    /// Interaction logic for ConfigurationEditControl.xaml
    /// </summary>
    public partial class ConfigurationEditControl : UserControl
    {
        private static readonly CsvFile DEFAULT_CSV_FILE = new CsvFile();

        private static EditGridControlViewModel VM;

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
                control.CsvFile as CsvFile,
                control.DataContext as EditorVM);

            Console.WriteLine($"ConfigurationEditControl, new CsvFile set.");

            var topContainer = control.TopContainer as Grid;
            topContainer.Children.Clear();

            topContainer.Children.Add(VM.GetEditConfigurationsGridForNewCsvFile());
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            VM.QueryForRelativePathToRootPathCommand.Execute();
        }
    }
}
