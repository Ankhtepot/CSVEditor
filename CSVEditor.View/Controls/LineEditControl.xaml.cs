using CSVEditor.Model;
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

        private static LineEditControlViewModel VM;

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
            VM = new LineEditControlViewModel(Resources);
        }

        private static void CsvFileChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (LineEditControl)d;
            var newValue = (CsvFile)e.NewValue;

            if (control == null || newValue == null)
            {
                return;
            }

            Console.WriteLine($"LineEditControl, new CsvFile set.");

            var csvFile = control.CsvFile as CsvFile;


            var topContainer = control.TopContainer as Grid;
            topContainer.Children.Clear();

            var mainGrid = new Grid()
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
            };

            //mainGrid.ShowGridLines = true;

            mainGrid = VM.GetMainGridForNewCsvFile(csvFile, mainGrid, control.SelectedLineIndex);

            topContainer.Children.Add(mainGrid);
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

            Console.WriteLine($"LineEditControl, new SelectedLIneIndex =  {newValue}.");
        }
    }
}
