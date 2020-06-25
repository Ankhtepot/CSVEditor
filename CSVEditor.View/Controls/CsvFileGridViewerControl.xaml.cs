using CSVEditor.Model;
using CSVEditor.Model.Services;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace CSVEditor.View.Controls
{
    /// <summary>
    /// Interaction logic for CsvFileGridViewerControl.xaml
    /// </summary>
    public partial class CsvFileGridViewerControl : UserControl
    {
        public CsvFile InputCsvFile
        {
            get { return (CsvFile)GetValue(InputCsvFileProperty); }
            set { SetValue(InputCsvFileProperty, value); }
        }
        public static readonly DependencyProperty InputCsvFileProperty =
            DependencyProperty.Register("InputCsvFile", typeof(CsvFile), typeof(CsvFileGridViewerControl), new PropertyMetadata(new CsvFile(), InputCsvFileChanged));

        private static void InputCsvFileChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (CsvFileGridViewerControl)d;
            var newValue = (CsvFile)e.NewValue;

            if (control == null || newValue == null)
            {
                return;
            }

            var gridView = (GridView)control.GridListView.View;
            control.GridListView.ItemsSource = newValue.Lines;
            var csvFile = newValue ?? null;
            
            if (csvFile != null)
            {
                gridView.Columns.Clear();
                for (int i = 0; i < csvFile.HeadersStrings.Count; i++)
                {
                    var gridViewColumn = new GridViewColumn();
                    var dataTemplate = new DataTemplate();

                    var gridFactory = new FrameworkElementFactory(typeof(Grid));
                    var borderFactory = new FrameworkElementFactory(typeof(Border));
                    var textBlockFactory = new FrameworkElementFactory(typeof(TextBlock));

                    dataTemplate.VisualTree = borderFactory;                    

                    var newBinding = new Binding($"[{i}]");

                    textBlockFactory.SetBinding(TextBlock.TextProperty, newBinding);
                    textBlockFactory.SetValue(ForegroundProperty, new SolidColorBrush(Colors.Black));
                    textBlockFactory.SetValue(HorizontalAlignmentProperty, HorizontalAlignment.Stretch);
                    textBlockFactory.SetValue(VerticalAlignmentProperty, VerticalAlignment.Top);

                    gridFactory.SetValue(WidthProperty, double.NaN);
                    gridFactory.SetValue(HorizontalAlignmentProperty, HorizontalAlignment.Stretch);
                    gridFactory.SetValue(MarginProperty, new Thickness(6, 2, 6, 2));
                    gridFactory.AppendChild(textBlockFactory);

                    borderFactory.SetValue(BorderBrushProperty, Brushes.Gray);
                    borderFactory.SetValue(BorderThicknessProperty, new Thickness(1,0,1,1));
                    borderFactory.SetValue(MarginProperty, new Thickness(-6,-2,-8,-2));
                    borderFactory.SetValue(HeightProperty, double.NaN);

                    borderFactory.AppendChild(gridFactory);

                    var newHeader = new GridViewColumnHeader();
                    newHeader.Content = csvFile.HeadersStrings[i];
                    newHeader.Width = double.NaN;

                    gridViewColumn.CellTemplate = dataTemplate;
                    gridViewColumn.Header = newHeader;
                    gridViewColumn.Width = double.NaN;
                    gridViewColumn.SetValue(HorizontalAlignmentProperty, HorizontalAlignment.Stretch);

                    gridView.Columns.Add(gridViewColumn);
                }
            }
        }

        public CsvFileGridViewerControl()
        {
            InitializeComponent();
        }

        void OnListViewItem_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.Handled)
                return;

            ListViewItem item = MyVisualTreeHelper.FindParent<ListViewItem>((DependencyObject)e.OriginalSource);
            if (item == null)
                return;

            if (item.Focusable && !item.IsFocused)
                item.Focus();
        }

        private void ListView_SelectedIndexChanged(object sender, SelectionChangedEventArgs e)
        {
            ScrollToSelectedItem((ListView)sender);
            
        }

        private void ListView_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            ScrollToSelectedItem((ListView)sender);
        }

        private void ScrollToSelectedItem(ListView listView)
        {
            listView.ScrollIntoView(listView.SelectedItem);
        }
    }
}
