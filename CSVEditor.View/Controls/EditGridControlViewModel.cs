using CSVEditor.Model.HelperClasses;
using CSVEditor.Model.Services;
using CSVEditor.View.Controls.DataCellElements;
using CSVEditor.View.Controls.EditGridCellElements;
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
using static CSVEditor.Model.HelperClasses.Enums;
using Constants = CSVEditor.Model.HelperClasses.Constants;

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
        public Grid MainGridContainer { get; set; }
        public ResourceDictionary Resources { get; set; }
        public int LineIndex { get => Context.SelectedItemIndex; }
        public DelegateCommand QueryForRelativePathToRootPathCommand { get; set; }
        public DelegateCommand<string> OpenDateFilterGuideWindowCommand { get; set; }        

        public EditGridControlViewModel(ResourceDictionary resources, EditorVM context, Grid mainGridContainer = null)
        {
            Resources = resources;
            Context = context;
            MainGridContainer = mainGridContainer;

            QueryForRelativePathToRootPathCommand = new DelegateCommand(QueryForRelativePathToRootPath);
            OpenDateFilterGuideWindowCommand = new DelegateCommand<string>(OpenDateFilterGuideWindow);
        }

        private void OpenDateFilterGuideWindow(string parameterInfo)
        {
            var parameters = parameterInfo.Split('|');
            var dateGuideWindow = new DateGuideWindow(
                new CellInfo() 
                {
                    Content = parameters[0],
                    ColumnNr = int.Parse(parameters[1])
                });

            dateGuideWindow.ShowDialog();
            CellInfo returnValue = dateGuideWindow.WindowResult;

            if (returnValue != null && returnValue.Content != parameters[0])
            {
                UpdateFileConfigurations(returnValue.Content, returnValue.ColumnNr, MainGridContainer); 
            }
        }

        private void QueryForRelativePathToRootPath()
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
            var newElement = new TextBlock()
            {
                Padding = new Thickness(5),
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Center,
                Text = $"#{count + 1}",
            };
            newElement.SetResourceReference(TextBlock.FontSizeProperty, Constants.BASE_FONT_SIZE_KEY);
            return newElement;
        }

        private UIElement HeadersColumnCreationMethod(int count)
        {
            var newElement = new TextBlock()
            {
                Padding = new Thickness(5),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Text = $"{Context.SelectedCsvFile.HeadersStrings[count]}",
            };
            newElement.SetResourceReference(TextBlock.FontSizeProperty, Constants.BASE_FONT_SIZE_KEY);
            return newElement;
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
                Margin = new Thickness(5),
                Height = 25
            };

            newComboBox.SelectionChanged += FieldTypeComboBox_SelectionChanged;

            return newComboBox;
        }

        private UIElement UriColumnCreationMethod(int count)
        {
            switch (Context.SelectedCsvFile.ColumnConfigurations[count].Type)
            {
                case (FieldType.Image):
                    {
                        var newUri = BuildConfigUriCell(Constants.IMAGE_URI_LABEL_TEXT, count);
                        newUri.TextBox.ContextMenu = (ContextMenu)Resources[URI_TEXT_BOX_CONTEXT_MENU];
                        newUri.TextBox.ContextMenuClosing += (sender, e) => lastTextBoxWithContextMenuClosed = sender as TextBox;

                        return newUri;
                    };
                case (FieldType.Date):
                    {
                        var wrapperGrid = BuildBasicGrid(1, 2);

                        var uriCell = BuildConfigUriCell(Constants.DATE_URI_LABEL_TEXT, count);
                        uriCell.Margin = ElementMargin;
                        Grid.SetColumn(uriCell, 0);
                        Grid.SetRow(uriCell, 0);
                        wrapperGrid.Children.Add(uriCell);

                        var uriCellTextBinding = new Binding("Text")
                        {
                            Source = uriCell.DataContext,
                            Mode = BindingMode.TwoWay
                        };

                        var filterInfoButton = new ImageButtonControl()
                        {
                            Width = 20,
                            Height = 20,
                            HorizontalAlignment = HorizontalAlignment.Center,
                            VerticalAlignment = VerticalAlignment.Center,
                            ImageSource = ResourceHelper.GetBitmapImageFromResources("Images/question-mark-160071_small.png"),
                            CornerRadius = new CornerRadius(10),
                            Command = OpenDateFilterGuideWindowCommand,
                            CommandParameter = $"{Context.SelectedCsvFile.ColumnConfigurations[count].URI}|{count}",
                            ToolTip = Constants.OPEN_DATE_FILTER_GUIDE_WINDOW_TOOLTIP,
                            Margin = ElementMargin
                        };
                        Grid.SetColumn(filterInfoButton, 1);
                        Grid.SetRow(filterInfoButton, 0);
                        wrapperGrid.Children.Add(filterInfoButton);

                        return wrapperGrid;
                    };
                default: return null;
            }
        }

        private LabeledTextBoxControl BuildConfigUriCell(string labelContent, int columnNr)
        {
            var newUri = new LabeledTextBoxControl()
            {
                HorizontalAlignment = HorizontalAlignment.Left,
            };

            newUri.TextBox.Tag = columnNr;
            newUri.Text = Context.SelectedCsvFile.ColumnConfigurations[columnNr].URI;
            newUri.TextBox.TextChanged += NewUri_TextChanged;
            newUri.LabelContent = labelContent;

            return newUri;
        }

        private void NewUri_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = (TextBox)sender;
            var columnNumber = (int)textBox.Tag;
            var newValue = textBox.Text;

            UpdateFileConfigurations(newValue, columnNumber);
        }

        private void FieldTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var columnNumber = (int)((ComboBox)sender).Tag;
            var newValue = (FieldType)e.AddedItems[0];
            
            UpdateFileConfigurations(newValue, columnNumber, MainGridContainer);
        }

        private void UpdateFileConfigurations(object updatedValue, int columnNr, Grid mainGridContainer = null)
        {
            var shouldUpdate = false;

            if (updatedValue is FieldType newType && newType != Context.SelectedCsvFile.ColumnConfigurations[columnNr].Type) 
            {
                Context.SelectedCsvFile.ColumnConfigurations[columnNr].Type = newType;
                shouldUpdate = true;
            }

            if (updatedValue is string newUri && newUri != Context.SelectedCsvFile.ColumnConfigurations[columnNr].URI)
            {
                Context.SelectedCsvFile.ColumnConfigurations[columnNr].URI = newUri;
                shouldUpdate = true;
            }

            if (shouldUpdate)
            {
                Context.UpdateFileConfigurations(mainGridContainer);
                return;
            }

            if (updatedValue !is string && updatedValue !is FieldType)
            {
                Console.WriteLine("Configuration wasnt updated, newValue not recognized"); 
            }
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

                if (newCellElement != null)
                {
                    Grid.SetColumn(newCellElement, columnNr);
                    Grid.SetRow(newCellElement, i);

                    MainGrid.Children.Add(newCellElement);
                }
            }
        }

        private UIElement CreateDataCellElement(FieldType type, int columnNr)
        {
            if (Context.SelectedCsvFile.Lines.Count <= 0)
            {
                return new UIElement();
            }

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
                        newElement.SetResourceReference(TextBox.FontSizeProperty, Constants.BASE_FONT_SIZE_KEY);
                        newElement.KeyDown += (sender, e) => Context.IsFileEdited = true;
                        newElement.SetBinding(TextBox.TextProperty, getBaseTwoWayBinding(columnNr));
                        return newElement;
                    };
                case FieldType.TextArea:
                    {
                        var newElement = new TextBox()
                        {
                            Name = $"DataCellTextAreaRow{LineIndex}Column{columnNr}",
                            Margin = ElementMargin,
                            AcceptsReturn = true,
                        };
                        newElement.SetResourceReference(TextBox.FontSizeProperty, Constants.BASE_FONT_SIZE_KEY);
                        newElement.KeyDown += (sender, e) => Context.IsFileEdited = true;
                        newElement.SetBinding(TextBox.TextProperty, getBaseTwoWayBinding(columnNr));
                        return newElement;
                    };
                case FieldType.Select:
                    {
                        var columnDistinctValues = GetColumnDistinctValues(columnNr);

                        var newElement = new SelectElementControl()
                        {
                            ComboBoxSource = columnDistinctValues,
                            Margin = ElementMargin
                        };
                        newElement.SetBinding(SelectElementControl.TextProperty, getBaseTwoWayBinding(columnNr));
                        newElement.ContentTextBox.KeyDown += (sender, e) => Context.IsFileEdited = true;
                        newElement.OnEdited += () => Context.IsFileEdited = true;
                        return newElement;
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
                        newElement.SetBinding(UriTextBoxControl.TextProperty, getBaseTwoWayBinding(columnNr));
                        newElement.UriTextBox.KeyDown += (sender, e) => Context.IsFileEdited = true;
                        return newElement;
                    };
                case FieldType.Date:
                    {
                        var newElement = new DateElementControl()
                        {
                            Name = $"DataCellDateRow{LineIndex}Column{columnNr}",
                            Margin = ElementMargin,
                            DateFormat = Context.SelectedCsvFile.ColumnConfigurations[columnNr].URI,
                        };

                        newElement.SetBinding(DateElementControl.TextProperty, getBaseTwoWayBinding(columnNr));
                        newElement.DateTextBox.KeyDown += (sender, e) => Context.IsFileEdited = true;
                        newElement.OnEdited += () => Context.IsFileEdited = true;
                        return newElement;
                    }
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
            newImageControl.CellContentTextBox.KeyDown += (sender, e) => Context.IsFileEdited = true;

            return newImageControl;
        }

        private List<string> GetColumnDistinctValues(int columnNr)
        {
            return Context.SelectedCsvFile.Lines
                .Select(Line => Line[columnNr])
                .Where(record => !string.IsNullOrEmpty(record))
                .Distinct()
                .ToList();
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
