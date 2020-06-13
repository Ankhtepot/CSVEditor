using CSVEditor.Model;
using CSVEditor.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using static CSVEditor.Model.Enums;

namespace CSVEditor.View.Controls
{
    public class EditGridControlViewModel
    {
        private const double MAX_COLUMN_WIDTH = 500d;
        private const double MIN_COLUMN_WIDTH = 30d;
        private const double IMAGE_WIDTH = 200d;
        private const double IMAGE_HEIGHT = 150d;

        public EditorVM Context { get; set; }
        public CsvFile CsvFile { get; set; }
        public Grid MainGrid { get; set; }
        public ResourceDictionary Resources { get; set; }
        public int LineIndex { get; set; }
        private int rowsCount { get => CsvFile.HeadersStrings.Count + 1; }

        public EditGridControlViewModel(ResourceDictionary resources, CsvFile csvFile, EditorVM context, int lineIndex = -1)
        {
            Resources = resources;
            CsvFile = csvFile;
            LineIndex = lineIndex;
            Context = context;
        }

        private void SetupNewGrid()
        {
            MainGrid = new Grid()
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
            };
        }

        public Grid GetEditLinesGridForNewCsvFile()
        {
            SetupNewGrid();

            AddRowsDefinitionsToGrid();
            AddColumnContent(0, RowNumberColumnCreationMethod, "Column\nNumber");
            AddColumnContent(1, HeadersColumnCreationMethod, "Fields");
            AddColumnContent(2, (i) => CreateDataCellElement(CsvFile.ColumnConfigurations[i].Type, i), "Data", "LeftAlignedHeaderTextBoxStyle");

            return MainGrid;
        }

        public Grid GetEditConfigurationsGridForNewCsvFile()
        {
            SetupNewGrid();

            AddRowsDefinitionsToGrid();
            AddColumnContent(0, RowNumberColumnCreationMethod, "Column\nNumber");
            AddColumnContent(1, HeadersColumnCreationMethod, "Column\nName");
            AddColumnContent(2, HeaderSelectionColumnCreationMethod, "Displayed\nColumn");
            AddColumnContent(3, ItemTypeSelectionColumnCreationMethod, "Field\nType");
            AddColumnContent(4, UriColumnCreationMethod, "URI", "LeftAlignedHeaderTextBoxStyle");

            return MainGrid;
        }

        private void AddRowsDefinitionsToGrid()
        {
            for (int i = 0; i < rowsCount + 1; i++) // +1 for header line
            {
                MainGrid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            }
        }

        private void AddColumnDefinitionToGrid()
        {
            MainGrid.ColumnDefinitions.Add(new ColumnDefinition()
            {
                Width = GridLength.Auto,
                MaxWidth = MAX_COLUMN_WIDTH,
                MinWidth = MIN_COLUMN_WIDTH
            });
        }

        private UIElement RowNumberColumnCreationMethod(int count)
        {
            return new TextBlock()
            {
                Padding = new Thickness(5),
                HorizontalAlignment = HorizontalAlignment.Right,
                Text = $"#{count + 1}",
            };
        }

        private UIElement HeadersColumnCreationMethod(int count)
        {
            return new TextBlock()
            {
                Padding = new Thickness(5),
                HorizontalAlignment = HorizontalAlignment.Center,
                Text = $"{CsvFile.HeadersStrings[count]}",
            };
        }

        private UIElement HeaderSelectionColumnCreationMethod(int count)
        {
            return new RadioButton()
            {
                GroupName = "HeaderSelectionRadioGroup",
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                IsChecked = count == 0
            };
        }

        private UIElement ItemTypeSelectionColumnCreationMethod(int count)
        {
            var newComboBox = new ComboBox()
            {
                Tag = count,
                ItemsSource = Enum.GetValues(typeof(FieldType)),
                SelectedIndex = (int)Enum.Parse(typeof(FieldType), CsvFile.ColumnConfigurations[count].Type.ToString()),
                Margin = new Thickness(5)
            };

            newComboBox.SelectionChanged += FieldTypeComboBox_SelectionChanged;

            return newComboBox;
        }

        private void FieldTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var context = ((ComboBox)sender).DataContext as EditorVM;
            context.SelectedCsvFile.ColumnConfigurations[(int)((ComboBox)sender).Tag].Type = (FieldType)e.AddedItems[0];
            CsvFile.ColumnConfigurations[(int)((ComboBox)sender).Tag].Type = (FieldType)e.AddedItems[0];
        }

        private UIElement UriColumnCreationMethod(int count)
        {
            return new TextBlock()
            {
                Padding = new Thickness(5),
                HorizontalAlignment = HorizontalAlignment.Left,
                Text = "TESTTESTTEST",// CsvFile.ColumnConfigurations[count - 1].URI,
                MinWidth = MIN_COLUMN_WIDTH
            };
        }

        private void AddColumnContent(
            int columnNr,
            Func<int, UIElement> creationMethod,
            string headerName,
            string columnHeaderStyle = "HeaderTextBlockStyle")
        {
            AddColumnDefinitionToGrid();

            MainGrid.Children.Add(BuildHeader(columnNr, headerName, columnHeaderStyle));

            for (int i = 1; i < rowsCount; i++)
            {
                var newCellElement = creationMethod(i - 1); //  -1 because index 0 is header

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

            switch (type)
            {
                case FieldType.TextBox:
                    {
                        newElement = new TextBox()
                        {
                            Margin = elementMargin,
                            Text = lineData[columnNr],
                            Tag = new ElementLocationTag() { rowNumber = columnNr, columnNumber = LineIndex }
                        };
                        (newElement as TextBox).TextChanged += TextBox_TextChanged;
                        return newElement;
                    };
                case FieldType.TextArea:
                    return new TextBox()
                    {
                        Margin = elementMargin,
                        Text = lineData[columnNr],
                        AcceptsReturn = true
                    };
                case FieldType.Select:
                    return new ComboBox()
                    {
                        Margin = elementMargin,
                        ItemsSource = GetColumnDistinctValues(columnNr)
                    };
                case FieldType.Image:
                    return new Image()
                    {
                        Margin = elementMargin,
                        Height = IMAGE_HEIGHT,
                        Width = IMAGE_WIDTH,
                        //TODO: add loading image from path
                    };
                case FieldType.URI:
                    return new TextBox()
                    {
                        Margin = elementMargin,
                    };
                default: throw new NotSupportedException($"Element type \"{type}\" not supported.");
            };
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = (TextBox)sender;
            var tagData = (ElementLocationTag)textBox.Tag;
            Context.SelectedCsvFile.Lines[tagData.rowNumber][tagData.columnNumber] = textBox.Text;
            CsvFile.Lines[tagData.rowNumber][tagData.columnNumber] = textBox.Text;
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
