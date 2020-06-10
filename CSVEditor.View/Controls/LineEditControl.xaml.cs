using CSVEditor.Model;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace CSVEditor.View.Controls
{
    /// <summary>
    /// Interaction logic for LineEditControler.xaml
    /// </summary>
    public partial class LineEditControl : UserControl
    {
        private static readonly CsvFile DEFAULT_CSV_FILE = new CsvFile();

        private static Style headerStyle;
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
            headerStyle = GetHeaderStyle();
        }

        private Style GetHeaderStyle()
        {
            var style = new Style(typeof(TextBlock));

            style.Setters.Add(new Setter(BackgroundProperty, Brushes.LightGray));
            style.Setters.Add(new Setter(FontWeightProperty, FontWeights.Bold));

                //Background = Brushes.LightCoral,
                //FontWeight = FontWeights.Bold,
                //Padding = new Thickness(10),
                //TextWrapping = TextWrapping.Wrap,
            return style;
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

            mainGrid.ShowGridLines = true;

            AddColumnsAndRowsDefinitionsToGrid(mainGrid, csvFile.HeadersStrings.Count);
            AddColumnNumberContentToGrid(mainGrid);

            topContainer.Children.Add(mainGrid);
        }

        private static void SelectedLineIndexChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (LineEditControl)d;
            var newValue = (int)e.NewValue;
            if (newValue == -1)
            {
                newValue = 0;
            }

            if (control == null || control.CsvFile == null)
            {
                Console.WriteLine($"LineEditControl:SelectedLineIndexChanged CsvFileSelected not selected or control is null.");
                return;
            }

            Console.WriteLine($"LineEditControl, new SelectedLIneIndex =  {newValue}.");
        }

        private static void AddColumnsAndRowsDefinitionsToGrid(Grid grid, int rowsCount)
        {

            for (int i = 0; i < rowsCount + 1; i++) // +1 for header line
            {
                grid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            }

            for (int i = 0; i < 3; i++) // columnNumber / columnName / Content
            {
                grid.ColumnDefinitions.Add(new ColumnDefinition()
                {
                    Width = GridLength.Auto, //OR can use Auto
                    MaxWidth = 300d,
                    MinWidth = 30d
                });
            }
        }

        private static void AddColumnNumberContentToGrid(Grid grid)
        {
            //Header TextBlock
            var newHeader = new TextBlock()
            {
                Background = Brushes.LightCoral,
                FontWeight = FontWeights.Bold,
                Padding = new Thickness(10),
                TextWrapping = TextWrapping.Wrap,
                Text = "Column\nNumber"
            };

            Grid.SetColumn(newHeader, 0);
            Grid.SetRow(newHeader, 0);

            grid.Children.Add(newHeader);

            for (int i = 1; i < grid.RowDefinitions.Count; i++) //  from 1 because index 0 is header row
            {
                var newLineNumber = new TextBlock()
                {
                    Padding = new Thickness(5),
                    HorizontalAlignment = HorizontalAlignment.Right,
                    Text = $"#{i}",
                };

                Grid.SetColumn(newLineNumber, 0);
                Grid.SetRow(newLineNumber, i);

                grid.Children.Add(newLineNumber);
            }
        }
    }
}
