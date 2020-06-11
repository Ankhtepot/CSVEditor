using CSVEditor.Model;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using static CSVEditor.Model.Enums;

namespace CSVEditor.View.Controls
{
    public class LineEditControlViewModel
    {
        public CsvFile CsvFile { get; set; }
        public Grid MainGrid { get; set; }
        public ResourceDictionary Resources { get; set; }
        public int LineIndex { get; set; }

        private const double MaxColumnWidth = 500d;
        private const double MinColumnWidth = 30d;

        public LineEditControlViewModel(ResourceDictionary resources)
        {
            Resources = resources;
        }

        public Grid SetMainGridForNewCsvFile(CsvFile csvFile, Grid grid, int lineIndex)
        {
            CsvFile = csvFile;
            MainGrid = grid;
            LineIndex = lineIndex;

            AddColumnsAndRowsDefinitionsToGrid(csvFile.HeadersStrings.Count);
            AddColumnNumberContentToGrid();
            AddHadersColumnContentToGrid();
            AddDataColumnContentToGrid();

            return MainGrid;
        }

        public void AddColumnNumberContentToGrid()
        {
            MainGrid.Children.Add(BuildHeader(0, "Column\nNumber"));

            for (int i = 1; i < MainGrid.RowDefinitions.Count; i++) //  from 1 because index 0 is header row
            {
                var newLineNumber = new TextBlock()
                {
                    Padding = new Thickness(5),
                    HorizontalAlignment = HorizontalAlignment.Right,
                    Text = $"#{i}",
                };

                Grid.SetColumn(newLineNumber, 0);
                Grid.SetRow(newLineNumber, i);

                MainGrid.Children.Add(newLineNumber);
            }
        }

        private void AddColumnsAndRowsDefinitionsToGrid(int rowsCount)
        {

            for (int i = 0; i < rowsCount + 1; i++) // +1 for header line
            {
                MainGrid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            }

            for (int i = 0; i < 2; i++) // columnNumber / fields
            {
                MainGrid.ColumnDefinitions.Add(new ColumnDefinition()
                {
                    Width = GridLength.Auto,
                    MaxWidth = MaxColumnWidth,
                    MinWidth = MinColumnWidth
                });
            }

            MainGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) }); // data
        }

        private void AddHadersColumnContentToGrid()
        {
            MainGrid.Children.Add(BuildHeader(1, "Field"));

            for (int i = 1; i < MainGrid.RowDefinitions.Count; i++)
            {
                var newLineNumber = new TextBlock()
                {
                    Padding = new Thickness(5),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Text = $"{CsvFile.HeadersStrings[i - 1]}", //  -1 because index 0 is header
                };

                Grid.SetColumn(newLineNumber, 1);
                Grid.SetRow(newLineNumber, i);

                MainGrid.Children.Add(newLineNumber);
            }
        }

        private void AddDataColumnContentToGrid()
        {
            MainGrid.Children.Add(BuildHeader(2, "Data"));

            for (int i = 1; i < MainGrid.RowDefinitions.Count; i++)
            {
                var newElement = CreateDataCellElement(FieldType.TextBox, i - 1);

                Grid.SetColumn(newElement, 2);
                Grid.SetRow(newElement, i);

                MainGrid.Children.Add(newElement);
            }
        }

        private UIElement CreateDataCellElement(FieldType type, int columnNr)
        {
            return new TextBox() { Text = CsvFile.Lines[LineIndex][columnNr]};
        }

        public UIElement BuildHeader(int columnNumber, string text)
        {
            var wrapperGrid = new Grid();
            wrapperGrid.Style = (Style)Resources["HeaderGridStyle"];

            var newHeader = new TextBlock();
            newHeader.Style = (Style)Resources["HeaderTextBlockStyle"];
            newHeader.Text = text;
            

            Grid.SetColumn(wrapperGrid, columnNumber);
            Grid.SetRow(wrapperGrid, 0);

            wrapperGrid.Children.Add(newHeader);

            return wrapperGrid;
        }
    }
}
