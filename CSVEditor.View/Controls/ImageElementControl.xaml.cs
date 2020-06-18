using CSVEditor.Model.Services;
using CSVEditor.ViewModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

namespace CSVEditor.View.Controls
{
    /// <summary>
    /// Interaction logic for ImageElementControl.xaml
    /// </summary>
    public partial class ImageElementControl : UserControl
    {
        public string ImageCellContent
        {
            get { return (string)GetValue(ImageCellContentProperty); }
            set { SetValue(ImageCellContentProperty, value); }
        }
        public static readonly DependencyProperty ImageCellContentProperty =
            DependencyProperty.Register("ImageCellContent", typeof(string), typeof(ImageElementControl), new PropertyMetadata(null, ImageSourceChanged));

        public int ColumnNr
        {
            get { return (int)GetValue(ColumnNrProperty); }
            set { SetValue(ColumnNrProperty, value); }
        }
        public static readonly DependencyProperty ColumnNrProperty =
            DependencyProperty.Register("ColumnNr", typeof(int), typeof(ImageElementControl), new PropertyMetadata(0));

        public int LineIndex
        {
            get { return (int)GetValue(LineIndexProperty); }
            set { SetValue(LineIndexProperty, value); }
        }
        public static readonly DependencyProperty LineIndexProperty =
            DependencyProperty.Register("LineIndex", typeof(int), typeof(ImageElementControl), new PropertyMetadata(0));



        private static void ImageSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (ImageElementControl)d;
            var imageCellContent = (string)e.NewValue;            

            if (control == null || imageCellContent == null)
            {
                return;
            }

            var Context = control.DataContext as EditorVM;

            var path = Regex.Replace(imageCellContent, "\r", "");
            path = Regex.Replace(path, "/", @"\");
            path = Path.Combine(Context.RootRepositoryPath, path.Substring(1));

            BitmapImage newImage;

            if (File.Exists(path))
            {
                newImage = new BitmapImage(new Uri(path));
            }
            else
            {
                newImage = ResourceHelper.LoadBitmapFromResource("images/no_image_available.png");
            }

            var cellContentBinding = new Binding($"SelectedCsvFile.Lines[{control.LineIndex}][{control.ColumnNr}]");
            cellContentBinding.Source = Context;
            cellContentBinding.Mode = BindingMode.TwoWay;
            cellContentBinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;

            control.UriContentTextBlock.Text = Context.SelectedCsvFile.ColumnConfigurations[control.ColumnNr].URI;
            control.CellContentTextBox.SetBinding(TextBox.TextProperty, cellContentBinding);
            control.ImageFromSource.Source = newImage;
        }

        public ImageElementControl()
        {
            InitializeComponent();
        }
    }
}
