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
        public static readonly string ROOT_DIRECTORY = "Root Directory";

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

            var newImage = GetImageSource(imageCellContent, Context.RootRepositoryPath);

            var cellContentBinding = new Binding($"SelectedCsvFile.Lines[{control.LineIndex}][{control.ColumnNr}]");
            cellContentBinding.Source = Context;
            cellContentBinding.Mode = BindingMode.TwoWay;
            cellContentBinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;

            var configUri = Context.SelectedCsvFile.ColumnConfigurations[control.ColumnNr].URI;

            control.UriContentTextBlock.Text = string.IsNullOrEmpty(configUri) 
                ? $"{ROOT_DIRECTORY}: {Context.RootRepositoryPath}" 
                : configUri;
            control.CellContentTextBox.SetBinding(TextBox.TextProperty, cellContentBinding);
            control.ImageFromSource.Source = newImage;
        }

        public static BitmapImage GetImageSource(string cellContent, string rootRepositoryPath)
        {
            string path = ConvertContentPathToSystemPath(cellContent);
            path = Path.Combine(rootRepositoryPath, path.Substring(1));

            return FileSystemServices.SetBitmapImageFromPath(path);
        }

        public static string ConvertContentPathToSystemPath(string imageCellContent)
        {
            var path = Regex.Replace(imageCellContent, "\r", "");
            path = Regex.Replace(path, "/", @"\");
            return path;
        }

        public ImageElementControl()
        {
            InitializeComponent();
        }

        private void CellContentTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ImageFromSource.Source = GetImageSource(((TextBox)sender).Text, (DataContext as EditorVM).RootRepositoryPath);
        }

        private void ImageFromSource_PreviewDrop(object sender, DragEventArgs e)
        {

        }

        private void ImageFromSource_Drop(object sender, DragEventArgs e)
        {

        }
    }
}
