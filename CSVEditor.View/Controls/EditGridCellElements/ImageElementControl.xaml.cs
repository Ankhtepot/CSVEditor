﻿using CSVEditor.Model.HelperClasses;
using CSVEditor.ViewModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace CSVEditor.View.Controls
{
    /// <summary>
    /// Interaction logic for ImageElementControl.xaml
    /// </summary>
    public partial class ImageElementControl : UserControl
    {
        public static readonly string ROOT_DIRECTORY = "Root Directory";

        private static string LastAcceptedImageSavePath;

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

        public ImageElementControl()
        {
            InitializeComponent();
        }

        private static void ImageSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (ImageElementControl)d;
            var imageCellContent = (string)e.NewValue;

            if (control == null || imageCellContent == null)
            {
                return;
            }

            var Context = control.DataContext as EditorVM;
            var configUri = Context.SelectedCsvFile.ColumnConfigurations[control.ColumnNr].URI;
            var newImage = GetImageSource(imageCellContent, Context.RootRepositoryPath, configUri);

            var cellContentBinding = new Binding($"SelectedCsvFile.Lines[{Context.SelectedItemIndex}][{control.ColumnNr}]");
            cellContentBinding.Source = Context;
            cellContentBinding.Mode = BindingMode.TwoWay;
            cellContentBinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;

            control.UriContentTextBlock.Text = string.IsNullOrEmpty(configUri)
                ? $"{ROOT_DIRECTORY}: {Context.RootRepositoryPath}"
                : configUri;

            control.CellContentTextBox.SetBinding(TextBox.TextProperty, cellContentBinding);

            control.ImageFromSource.Source = newImage;
        }

        public static BitmapImage GetImageSource(string cellContent, string rootRepositoryPath, string configUri)
        {
            string path = FileSystemServices.ConvertContentPathToSystemPath(cellContent);
            string uriPath = configUri;
            path = Path.Combine(string.IsNullOrEmpty(configUri) ? rootRepositoryPath : uriPath, path);

            return FileSystemServices.GetBitmapImageFromPath(path);
        }

        private void CellContentTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var Context = DataContext as EditorVM;
            if (ColumnNr < Context.SelectedCsvFile.ColumnCount)
            {
                var uriText = Context.SelectedCsvFile.ColumnConfigurations[ColumnNr].URI;                

                ImageFromSource.Source = GetImageSource(
                    ((TextBox)sender).Text,
                    Context.RootRepositoryPath,
                    uriText); 
            }
        }

        private void ImageFromSource_PreviewDrop(object sender, DragEventArgs e)
        {
            string newImageFile = ((string[])e.Data.GetData(DataFormats.FileDrop))[0];

            e.Handled = SaveNewImageSourceFile(newImageFile);
        }

        private bool SaveNewImageSourceFile(string newImageFile)
        {
            if (FileSystemServices.IsImageFile(newImageFile))
            {
                var Context = DataContext as EditorVM;

                LastAcceptedImageSavePath ??= LastAcceptedImageSavePath = Context.RootRepositoryPath;
                
                var uriText = Context.SelectedCsvFile.ColumnConfigurations[ColumnNr].URI;

                var newImageFileName = Path.GetFileName(newImageFile);

                var selectedSavePath = string.IsNullOrEmpty(uriText)
                    ? FileSystemServices.QueryUserForPath(LastAcceptedImageSavePath, $"Save {newImageFileName} File to:")
                    : uriText;

                if (selectedSavePath == null)
                {
                    return false;
                }

                if (!string.IsNullOrEmpty(CellContentTextBox.Text))
                {
                    var replaceWindow = new ReplaceImageWindow();
                    //TODO: Fix savePath so its full path where file should be saved
                    replaceWindow.SetImagePaths(newImageFile, selectedSavePath, CellContentTextBox.Text);
                    replaceWindow.ShowDialog();
                }

                if (FileSystemServices.SaveImageFile(newImageFile, selectedSavePath))
                {
                    ResolveCellContentTextBoxFromSavePath(selectedSavePath, newImageFileName);
                };
            }

            return true;
        }

        private void ResolveCellContentTextBoxFromSavePath(string fileSavePath, string fileName)
        {
            var cellContentPath = Path.Combine(fileSavePath, fileName);
            var Context = DataContext as EditorVM;
            var uriText = Context.SelectedCsvFile.ColumnConfigurations[ColumnNr].URI;

            cellContentPath = string.IsNullOrEmpty(uriText)
                ? cellContentPath.Replace(Context.RootRepositoryPath, "").Replace("\\", "/")
                : cellContentPath.Replace(uriText + "\\", "").Replace("\\", "/");

            CellContentTextBox.Text = cellContentPath;
            Context.IsFileEdited = true;
        }

        private void ImageFromSource_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            SelectImageFileFromDialog();
        }

        private void SelectFileMenuItem_Click(object sender, RoutedEventArgs e)
        {
            SelectImageFileFromDialog();
        }

        private void SelectImageFileFromDialog()
        {
            var path = Path.Combine((DataContext as EditorVM).RootRepositoryPath, FileSystemServices.ConvertContentPathToSystemPath(CellContentTextBox.Text));
            path = Path.GetDirectoryName(path);
            var filter = Properties.Resources.ImageFilesFilter;
            var selectedImage = FileSystemServices.QueryUserToSelectFile(path, Constants.REPLACE_IMAGE_FILE, filter);

            if (!string.IsNullOrEmpty(selectedImage))
            {
                if (path == Path.GetDirectoryName(selectedImage))
                {
                    ResolveCellContentTextBoxFromSavePath(path, Path.GetFileName(selectedImage));
                    return;
                }

                SaveNewImageSourceFile(selectedImage);
            }
        }

        private void CellContentTextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            (DataContext as EditorVM).IsFileEdited = true;
        }
    }
}
