using CSVEditor.Model;
using CSVEditor.ViewModel;
using System;
using System.Windows;
using System.Windows.Controls;

namespace CSVEditor.View.Controls
{
    /// <summary>
    /// Interaction logic for LineEditControler.xaml
    /// </summary>
    public partial class LineEditControl : UserControl
    {
        private static readonly CsvFile DEFAULT_CSV_FILE = new CsvFile();

        private static EditGridControlViewModel VM;

        public CsvFile CsvFile
        {
            get { return (CsvFile)GetValue(CsvFileProperty); }
            set { SetValue(CsvFileProperty, value); }
        }
        public static readonly DependencyProperty CsvFileProperty =
            DependencyProperty.Register("CsvFile", typeof(CsvFile), typeof(LineEditControl), new PropertyMetadata(DEFAULT_CSV_FILE, CsvFileChanged));

        public int SelectedLineIndex
        {
            get { return (int)GetValue(SelectedLineIndexProperty); }
            set { SetValue(SelectedLineIndexProperty, value); }
        }
        public static readonly DependencyProperty SelectedLineIndexProperty =
            DependencyProperty.Register("SelectedLineIndex", typeof(int), typeof(LineEditControl), new PropertyMetadata(0, SelectedLineIndexChanged));

        public LineEditControl()
        {
            InitializeComponent();
        }

        private static void CsvFileChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (LineEditControl)d;
            var newValue = (CsvFile)e.NewValue;

            if (control == null || newValue == null)
            {
                return;
            }

            VM = new EditGridControlViewModel(
                control.Resources,
                control.CsvFile as CsvFile,
                control.DataContext as EditorVM,
                control.SelectedLineIndex);

            Console.WriteLine($"LineEditControl, new CsvFile set.");

            var topContainer = control.TopContainer as Grid;
            
            BuildGrid(topContainer);
        }

        private static void SelectedLineIndexChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (LineEditControl)d;
            var newValue = (int)e.NewValue;
            if (newValue == -1)
            {
                control.SelectedLineIndex = 0;
            }

            if (control == null || control.CsvFile == null)
            {
                Console.WriteLine($"LineEditControl:SelectedLineIndexChanged CsvFileSelected not selected or control is null.");
                return;
            }

            VM.LineIndex = control.SelectedLineIndex;
            Console.WriteLine($"LineEditControl, new SelectedLIneIndex =  {newValue}.");

            BuildGrid(control.TopContainer);
        }

        private void TopContainer_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue == true)
            {
                BuildGrid(TopContainer);
            }
        }

        private static void BuildGrid(Grid topContainer)
        {
            topContainer.Children.Clear();
            topContainer.Children.Add(VM.GetEditLinesGridForNewCsvFile());
        }
    }
}
