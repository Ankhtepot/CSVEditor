using CSVEditor.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using static CSVEditor.Model.Enums;

namespace CSVEditor.View.Controls
{
    public class LineEditControlViewModel
    {
        private const double MAX_COLUMN_WIDTH = 500d;
        private const double MIN_COLUMN_WIDTH = 30d;
        private const double IMAGE_WIDTH = 200d;
        private const double IMAGE_HEIGHT = 150d;

        public CsvFile CsvFile { get; set; }
        public Grid MainGrid { get; set; }
        public ResourceDictionary Resources { get; set; }
        public int LineIndex { get; set; }
        private int rowsCount { get => CsvFile.HeadersStrings.Count + 1; }

        public LineEditControlViewModel(ResourceDictionary resources, CsvFile csvFile, int lineIndex = -1)
        {
            Resources = resources;
            CsvFile = csvFile;
            LineIndex = lineIndex;

            MainGrid = new Grid()
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
            };

            
        }

        public Grid GetEditLinesGridForNewCsvFile()
        {
            AddColumnsAndRowsDefinitionsToGrid(3);
            AddColumnContent(0, RowNumberColumnCreationMethod, "Column\nNumber");
            AddColumnContent(1, HeadersColumnCreationMethod, "Fields");
            AddColumnContent(2, (i) => CreateDataCellElement(FieldType.TextBox, i - 1), "Data", "LeftAlignedHeaderTextBoxStyle");

            return MainGrid;
        }

        public Grid GetEditConfigurationsGridForNewCsvFile()
        {
            AddColumnsAndRowsDefinitionsToGrid(4);
            AddColumnContent(0, RowNumberColumnCreationMethod, "Column\nNumber");
            AddColumnContent(1, HeadersColumnCreationMethod, "Column\nNames");

            return MainGrid;
        }

        private void AddColumnsAndRowsDefinitionsToGrid(int columnCount)
        {

            for (int i = 0; i < rowsCount + 1; i++) // +1 for header line
            {
                MainGrid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            }

            for (int i = 0; i < columnCount; i++) // columnNumber / fields
            {
                MainGrid.ColumnDefinitions.Add(new ColumnDefinition()
                {
                    Width = GridLength.Auto,
                    MaxWidth = MAX_COLUMN_WIDTH,
                    MinWidth = MIN_COLUMN_WIDTH
                });
            }
        }

        private UIElement RowNumberColumnCreationMethod(int count)
        {
            return new TextBlock()
            {
                Padding = new Thickness(5),
                HorizontalAlignment = HorizontalAlignment.Right,
                Text = $"#{count}",
            };
        }

        private UIElement HeadersColumnCreationMethod(int count)
        {
            return new TextBlock()
            {
                Padding = new Thickness(5),
                HorizontalAlignment = HorizontalAlignment.Center,
                Text = $"{CsvFile.HeadersStrings[count - 1]}", //  -1 because index 0 is header
            };
        }

        private void AddColumnContent(int columnNr, Func<int, UIElement> creationMethod, string headerName, string columnHeaderStyle = "HeaderTextBlockStyle")
        {
            MainGrid.Children.Add(BuildHeader(columnNr, headerName, columnHeaderStyle));

            for (int i = 1; i < rowsCount; i++)
            {
                var newCellElement = creationMethod(i);

                Grid.SetColumn(newCellElement, columnNr);
                Grid.SetRow(newCellElement, i);

                MainGrid.Children.Add(newCellElement);
            }
        }

        private UIElement CreateDataCellElement(FieldType type, int columnNr)
        {
            var lineData = CsvFile.Lines[LineIndex];
            var newElement = new UIElement();
            var elementMargin = new Thickness(2d);

            return type switch
            {
                FieldType.TextBox => new TextBox() 
                {
                    Margin = elementMargin,
                    Text = lineData[columnNr] 
                },
                FieldType.TextArea => new TextBox()
                {
                    Margin = elementMargin,
                    Text = lineData[columnNr],
                    TextWrapping = TextWrapping.Wrap
                },
                FieldType.Select => new ComboBox()
                {
                    Margin = elementMargin,
                    ItemsSource = GetColumnDistinctValues(columnNr)
                },
                FieldType.Image => new Image()
                {
                    Margin = elementMargin,
                    Height = IMAGE_HEIGHT,
                    Width = IMAGE_WIDTH,
                    //TODO: add loading image from path
                },
                FieldType.URI => new TextBox()
                {
                    Margin = elementMargin,
                },
                _ => throw new NotSupportedException($"Element type \"{type}\" not supported.")
            };
             
        }

        private List<string> GetColumnDistinctValues(int columnNr)
        {
            return CsvFile.Lines.Select(Line => Line[columnNr]).Distinct().ToList();
        }

        //TODO: Use constants instead direct strings both in xaml and here
        public UIElement BuildHeader(int columnNumber, string text, string headerStyle = "HeaderTextBlockStyle")
        {
            var wrapperGrid = new Grid();
            wrapperGrid.Style = (Style)Resources["HeaderGridStyle"];

            var newHeader = new TextBlock();
            newHeader.Style = (Style)Resources[headerStyle];
            newHeader.Text = text;
            

            Grid.SetColumn(wrapperGrid, columnNumber);
            Grid.SetRow(wrapperGrid, 0);

            wrapperGrid.Children.Add(newHeader);

            return wrapperGrid;
        }
    }
}
