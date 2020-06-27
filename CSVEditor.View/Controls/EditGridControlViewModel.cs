using CSVEditor.Model;
using CSVEditor.Model.Services;
using CSVEditor.ViewModel;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using static CSVEditor.Model.Enums;

namespace CSVEditor.View.Controls
{
    public class EditGridControlViewModel : INotifyPropertyChanged
    {
        private int rowsCount { get => Context.SelectedCsvFile.HeadersStrings.Count + 1; }
        
        private TextBox lastTextBoxWithContextMenuClosed;

        public const string HEADER_TEXT_BOX_STYLE = "HeaderTextBlockStyle";
        public const string LEFT_ALIGNED_HEADER_TEXT_BOX_STYLE = "LeftAlignedHeaderTextBoxStyle";
        public const string HEADER_GRID_STYLE = "HeaderGridStyle";
        public const string URI_TEXT_BOX_CONTEXT_MENU = "UriTextBoxContextMenu";

        public double MaxColumnWidth { get; set; } = 1000d;
        public double MinColumnWidth { get; set; } = 30d;
        public double ImageWidth { get; set; } = 200d;
        public double ImageHeight { get; set; } = 150d;
        public Thickness ElementMargin { get; set; } = new Thickness(2d);
        public EditorVM Context { get; set; }
        public Grid MainGrid { get; set; }
        public ResourceDictionary Resources { get; set; }
        public int LineIndex { get => Context.SelectedItemIndex; }        

        public DelegateCommand QueryForRelativePathToRootPathCommand { get; set; }

        public EditGridControlViewModel(ResourceDictionary resources, EditorVM context)
        {
            Resources = resources;
            Context = context;

            QueryForRelativePathToRootPathCommand = new DelegateCommand(queryForRelativePathToRootPath);
        }

        private void queryForRelativePathToRootPath()
        {
            var text = FileSystemServices.QueryUserForPath(Context.SelectedCsvFile.AbsPath, Constants.SELECT_PREDEFINED_SAVE_PATH);
            lastTextBoxWithContextMenuClosed.Text = text;
        }

        private void setupNewGrid()
        {
            MainGrid = new Grid()
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
            };
        }

        public Grid GetEditLinesGridForNewCsvFile()
        {            
            setupNewGrid();

            if (LineIndex == -1)
            {
                return MainGrid;
            }

            AddRowsDefinitionsToGrid();
            AddColumnWithContent(0, RowNumberColumnCreationMethod, "Column\nNumber");
            AddColumnWithContent(1, HeadersColumnCreationMethod, "Fields");
            AddColumnWithContent(2,
                (i) => CreateDataCellElement(Context.SelectedCsvFile.ColumnConfigurations[i].Type, i),
                "Data",
                LEFT_ALIGNED_HEADER_TEXT_BOX_STYLE,
                true);

            return MainGrid;
        }

        public Grid GetEditConfigurationsGridForNewCsvFile()
        {
            setupNewGrid();

            AddRowsDefinitionsToGrid();
            AddColumnWithContent(0, RowNumberColumnCreationMethod, "Column\nNumber");
            AddColumnWithContent(1, HeadersColumnCreationMethod, "Column\nName");
            AddColumnWithContent(2, HeaderSelectionColumnCreationMethod, "Displayed\nColumn");
            AddColumnWithContent(3, ItemTypeSelectionColumnCreationMethod, "Field\nType");
            AddColumnWithContent(4, UriColumnCreationMethod, "URI", LEFT_ALIGNED_HEADER_TEXT_BOX_STYLE, true);

            return MainGrid;
        }

        private void AddRowsDefinitionsToGrid()
        {
            for (int i = 0; i < rowsCount + 1; i++) // +1 for header line
            {
                MainGrid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            }
        }

        private void AddColumnDefinitionToGrid(bool isLastColumn = false)
        {
            var newDefinition = new ColumnDefinition()
            {
                Width = isLastColumn
                  ? new GridLength(1, GridUnitType.Star)
                  : GridLength.Auto,
                MinWidth = MinColumnWidth
            };

            if (!isLastColumn)
            {
                newDefinition.MaxWidth = MaxColumnWidth;
            }

            MainGrid.ColumnDefinitions.Add(newDefinition);
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
                Text = $"{Context.SelectedCsvFile.HeadersStrings[count]}",
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
                SelectedIndex = (int)Enum.Parse(typeof(FieldType), Context.SelectedCsvFile.ColumnConfigurations[count].Type.ToString()),
                Margin = new Thickness(5)
            };

            newComboBox.SelectionChanged += FieldTypeComboBox_SelectionChanged;

            return newComboBox;
        }

        private UIElement UriColumnCreationMethod(int count)
        {
            var newUri = new TextBox()
            {
                Margin = new Thickness(5),
                HorizontalAlignment = HorizontalAlignment.Left,
                Text = Context.SelectedCsvFile.ColumnConfigurations[count].URI,
                MinWidth = MinColumnWidth * 3,
                Tag = count
            };

            newUri.ContextMenu = (ContextMenu)Resources[URI_TEXT_BOX_CONTEXT_MENU];
            newUri.ContextMenuClosing += (sender, e) => lastTextBoxWithContextMenuClosed = sender as TextBox;
            newUri.TextChanged += NewUri_TextChanged;

            return newUri;
        }

        private void NewUri_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = (TextBox)sender;
            var columnNumber = (int)textBox.Tag;
            var newValue = textBox.Text;

            Context.SelectedCsvFile.ColumnConfigurations[columnNumber].URI = newValue;
            Context.UpdateFileConfigurations(Context.SelectedCsvFile.ColumnConfigurations, Context.SelectedCsvFile.AbsPath);
        }

        private void FieldTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var columnNumber = (int)((ComboBox)sender).Tag;
            var newValue = (FieldType)e.AddedItems[0];

            Context.SelectedCsvFile.ColumnConfigurations[columnNumber].Type = newValue;
            Context.UpdateFileConfigurations(Context.SelectedCsvFile.ColumnConfigurations, Context.SelectedCsvFile.AbsPath);
        }

        private void AddColumnWithContent(
            int columnNr,
            Func<int, UIElement> creationMethod,
            string headerName,
            string columnHeaderStyle = HEADER_TEXT_BOX_STYLE,
            bool isLastColumn = false
            )
        {
            AddColumnDefinitionToGrid(isLastColumn);

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
            var columnContent = Context.SelectedCsvFile.Lines[LineIndex][columnNr];

            switch (type)
            {
                case FieldType.TextBox:
                    {
                        var newElement = new TextBox()
                        {
                            Name = $"DataCellTextBoxRow{LineIndex}Column{columnNr}",
                            Margin = ElementMargin,
                        };

                        (newElement as TextBox).SetBinding(TextBox.TextProperty, getBaseTwoWayBinding(columnNr));
                        return newElement;
                    };
                case FieldType.TextArea:
                    {
                        var newElement = new TextBox()
                        {
                            Name = $"DataCellTextAreaRow{LineIndex}Column{columnNr}",
                            Margin = ElementMargin,
                            AcceptsReturn = true
                        };
                        (newElement as TextBox).SetBinding(TextBox.TextProperty, getBaseTwoWayBinding(columnNr));
                        return newElement;
                    };
                case FieldType.Select:
                    {
                        var columnDistinctValues = GetColumnDistinctValues(columnNr);

                        return new ComboBox()
                        {
                            Margin = ElementMargin,
                            ItemsSource = columnDistinctValues,
                            SelectedIndex = columnDistinctValues.FindIndex(record => record == columnContent),
                            MaxWidth = MaxColumnWidth,
                            HorizontalAlignment = HorizontalAlignment.Left,
                        };
                    };
                case FieldType.Image:
                    {
                        return BuildImageElementControl(columnNr, columnContent);
                    }
                case FieldType.URI:
                    {
                        var newElement = new UriTextBoxControl()
                        {
                            Name = $"DataCellUriRow{LineIndex}Column{columnNr}",
                            Margin = ElementMargin,
                        };

                        (newElement as UriTextBoxControl).SetBinding(UriTextBoxControl.TextProperty, getBaseTwoWayBinding(columnNr));
                        return newElement;
                    };
                default: throw new NotSupportedException($"Element type \"{type}\" not supported.");
            };
        }

        private Binding getBaseTwoWayBinding(int columnNr)
        {
            var binding = new Binding($"SelectedCsvFile.Lines[{LineIndex}][{columnNr}]");
            binding.Source = Context;
            binding.Mode = BindingMode.TwoWay;
            binding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;

            return binding;
        }

        private Binding getBaseBinding(int columnNr)
        {
            var binding = new Binding($"SelectedCsvFile.Lines[{LineIndex}][{columnNr}]");
            binding.Source = Context;

            return binding;
        }

        private UIElement BuildImageElementControl(int columnNr, string imageCellContent)
        {
            var newImageControl = new ImageElementControl()
            {
                DataContext = Context,
                MaxHeight = ImageHeight,
                ColumnNr = columnNr
            };

            var itemIndexbinding = new Binding("SelectedItemIndex");
            itemIndexbinding.Source = Context;

            newImageControl.SetBinding(ImageElementControl.ImageCellContentProperty, getBaseTwoWayBinding(columnNr));

            return newImageControl;
        }

        private List<string> GetColumnDistinctValues(int columnNr)
        {
            return Context.SelectedCsvFile.Lines.Select(Line => Line[columnNr]).Distinct().ToList();
        }

        public UIElement BuildHeader(int columnNumber, string text, string headerStyle = HEADER_TEXT_BOX_STYLE)
        {
            var wrapperGrid = new Grid();
            wrapperGrid.Style = (Style)Resources[HEADER_GRID_STYLE];

            var newHeader = new TextBlock();
            newHeader.Style = (Style)Resources[headerStyle];
            newHeader.Text = text;


            Grid.SetColumn(wrapperGrid, columnNumber);
            Grid.SetRow(wrapperGrid, 0);

            wrapperGrid.Children.Add(newHeader);

            return wrapperGrid;
        }

        public Grid BuildBasicGrid(int rowCount, int columnCount)
        {
            var newGrid = new Grid();

            newGrid.HorizontalAlignment = HorizontalAlignment.Stretch;

            for (int i = 0; i < rowCount; i++)
            {
                newGrid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            }

            for (int i = 0; i < columnCount; i++)
            {
                newGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
            }

            return newGrid;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
