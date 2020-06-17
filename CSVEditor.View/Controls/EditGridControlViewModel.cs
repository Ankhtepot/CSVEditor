﻿using CSVEditor.Model;
using CSVEditor.Model.Services;
using CSVEditor.ViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using static CSVEditor.Model.Enums;

namespace CSVEditor.View.Controls
{
    public class EditGridControlViewModel : INotifyPropertyChanged
    {
        public const string HEADER_TEXT_BOX_STYLE = "HeaderTextBlockStyle";
        public const string LEFT_ALIGNED_HEADER_TEXT_BOX_STYLE = "LeftAlignedHeaderTextBoxStyle";
        public const string HEADER_GRID_STYLE = "HeaderGridStyle";

        public double MaxColumnWidth { get; set; } = 500d;
        public double MinColumnWidth { get; set; } = 30d;
        public double ImageWidth { get; set; } = 200d;
        public double ImageHeight { get; set; } = 150d;
        public Thickness ElementMargin { get; set; } = new Thickness(2d);

        private EditorVM context;
        public EditorVM Context
        {
            get { return context; }
            set
            {
                context = value;
                OnPropertyChanged();
            }
        }

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
            AddColumnContent(2, (i) => CreateDataCellElement(CsvFile.ColumnConfigurations[i].Type, i), "Data", LEFT_ALIGNED_HEADER_TEXT_BOX_STYLE);

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
            AddColumnContent(4, UriColumnCreationMethod, "URI", LEFT_ALIGNED_HEADER_TEXT_BOX_STYLE);

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
                MaxWidth = MaxColumnWidth,
                MinWidth = MinColumnWidth
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

        private UIElement UriColumnCreationMethod(int count)
        {
            var newUri = new TextBox()
            {
                Padding = new Thickness(5),
                HorizontalAlignment = HorizontalAlignment.Left,
                Text = CsvFile.ColumnConfigurations[count].URI,
                MinWidth = MinColumnWidth,
                Tag = count
            };

            newUri.TextChanged += NewUri_TextChanged;

            return newUri;
        }

        private void NewUri_TextChanged(object sender, TextChangedEventArgs e)
        {
            var columnNumber = (int)((TextBox)sender).Tag;
            var context = ((TextBox)sender).DataContext as EditorVM;
            var newValue = ((TextBox)sender).Text;

            context.SelectedCsvFile.ColumnConfigurations[columnNumber].URI = newValue;
            CsvFile.ColumnConfigurations[columnNumber].URI = newValue;
            context.UpdateFileConfigurations(CsvFile.ColumnConfigurations, CsvFile.AbsPath);
        }

        private void FieldTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var columnNumber = (int)((ComboBox)sender).Tag;
            var context = ((ComboBox)sender).DataContext as EditorVM;
            var newValue = (FieldType)e.AddedItems[0];

            context.SelectedCsvFile.ColumnConfigurations[columnNumber].Type = newValue;
            CsvFile.ColumnConfigurations[columnNumber].Type = newValue;
            context.UpdateFileConfigurations(CsvFile.ColumnConfigurations, CsvFile.AbsPath);
        }

        private void AddColumnContent(
            int columnNr,
            Func<int, UIElement> creationMethod,
            string headerName,
            string columnHeaderStyle = HEADER_TEXT_BOX_STYLE)
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
            var columnContent = CsvFile.Lines[LineIndex][columnNr];

            switch (type)
            {
                case FieldType.TextBox:
                    {
                        var newElement = new TextBox()
                        {
                            Margin = ElementMargin,
                            Tag = new ElementLocationTag() { rowNumber = columnNr, columnNumber = LineIndex }
                        };
                        var newTextBox = newElement as TextBox;
                        
                        newTextBox.SetBinding(TextBox.TextProperty, getBaseBinding(columnNr));
                        return newElement;
                    };
                case FieldType.TextArea:
                    {
                        var newElement = new TextBox()
                        {
                            Margin = ElementMargin,
                            Text = columnContent,
                            AcceptsReturn = true
                        };
                        (newElement as TextBox).SetBinding(TextBox.TextProperty, getBaseBinding(columnNr));
                        return newElement;
                    };
                case FieldType.Select:
                    return new ComboBox()
                    {
                        Margin = ElementMargin,
                        ItemsSource = GetColumnDistinctValues(columnNr)
                    };
                case FieldType.Image:
                    {

                        return BuildImageElementControl(columnContent);
                    }
                case FieldType.URI:
                    return new TextBox()
                    {
                        Margin = ElementMargin,
                    };
                default: throw new NotSupportedException($"Element type \"{type}\" not supported.");
            };
        }

        private Binding getBaseBinding(int columnNr)
        {
            var binding = new Binding($"SelectedCsvFile.Lines[{LineIndex}][{columnNr}]");
            binding.Source = Context;
            binding.Mode = BindingMode.TwoWay;
            binding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;

            return binding;
        }

        private UIElement BuildImageElementControl(string imagePath)
        {
            var path = Regex.Replace(imagePath, "\r", "");
            path = Regex.Replace(path, "/", @"\");
            path = Path.Combine(Context.RootRepositoryPath, path.Substring(1));

            //var wrapGrid = BuildBasicGrid(3, 2);

            //var newImage = new Image()
            //{
            //    Margin = ElementMargin,
            //    Height = ImageHeight,
            //    Width = ImageWidth,                
            //};

            BitmapImage newImage;

            if (File.Exists(path))
            {
                newImage = new BitmapImage(new Uri(path));
            }
            else
            {
                newImage = ResourceHelper.LoadBitmapFromResource("images/no_image_available.png");
            }
            //Grid.SetColumn(newImage, 0);
            //Grid.SetRow(newImage, 0);
            //Grid.SetRowSpan(newImage, 3);

            //return newImage;

            return new ImageElementControl() { Image = newImage, MaxHeight = ImageHeight};
        }

        private List<string> GetColumnDistinctValues(int columnNr)
        {
            return CsvFile.Lines.Select(Line => Line[columnNr]).Distinct().ToList();
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
