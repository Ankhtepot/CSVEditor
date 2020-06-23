﻿using CSVEditor.Model;
using CSVEditor.ViewModel;
using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace CSVEditor.View.Controls
{
    /// <summary>
    /// Interaction logic for ImageElementControl.xaml
    /// </summary>
    public partial class ImageElementControl : UserControl
    {
        public static readonly string ROOT_DIRECTORY = "Root Directory";

        private static string lastAcceptedImageSavePath;

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

            var cellContentBinding = new Binding($"SelectedCsvFile.Lines[{control.LineIndex}][{control.ColumnNr}]");
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
            string path = ConvertContentPathToSystemPath(cellContent);
            string uriPath = configUri;
            path = Path.Combine(string.IsNullOrEmpty(configUri) ? rootRepositoryPath : uriPath, path);

            return FileSystemServices.SetBitmapImageFromPath(path);
        }

        public static string ConvertContentPathToSystemPath(string imageCellContent)
        {
            var path = "";

            if (!string.IsNullOrEmpty(imageCellContent))
            {
                path = Regex.Replace(imageCellContent, "\r", "");
                path = Regex.Replace(path, "/", @"\");

                if (!char.IsLetterOrDigit(path[0]))
                {
                    path = path.Substring(1);
                } 
            }

            return path;
        }

        private void CellContentTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var Context = DataContext as EditorVM;
            var uriText = Context.SelectedCsvFile.ColumnConfigurations[ColumnNr].URI;
            ImageFromSource.Source = GetImageSource(
                ((TextBox)sender).Text,                
                Context.RootRepositoryPath,
                uriText);
        }

        private void ImageFromSource_PreviewDrop(object sender, DragEventArgs e)
        {
            string newImageFile = ((string[])e.Data.GetData(DataFormats.FileDrop))[0];
            
            if (FileSystemServices.IsImageFile(newImageFile))
            {
                lastAcceptedImageSavePath ??= lastAcceptedImageSavePath = (DataContext as EditorVM).RootRepositoryPath;

                var Context = DataContext as EditorVM;
                var uriText = Context.SelectedCsvFile.ColumnConfigurations[ColumnNr].URI;

                var newImageFileName = Path.GetFileName(newImageFile);

                var selectedSavePath = string.IsNullOrEmpty(uriText)
                    ? FileSystemServices.QueryUserForPath(lastAcceptedImageSavePath, $"Save {newImageFileName} File to:")
                    : uriText;

                if (selectedSavePath == null)
                {
                    e.Handled = false;
                    return;
                }

                if (saveDraggedFile(newImageFile, selectedSavePath))
                {
                    resolveCellContentTextBoxFromSavePath(selectedSavePath, newImageFileName);
                };
            }

            e.Handled = true;
        }

        private bool saveDraggedFile(string newImageFile, string selectedSavePath)
        {
            var newFileName = Path.GetFileName(newImageFile);

            var title = "Confirm Saving Image File";
            var message = $"Are you sure you want to save file {newFileName} to:\n{selectedSavePath} ?";
            var icon = MessageBoxImage.Warning;
            var buttons = MessageBoxButton.OKCancel;

            if (MessageBox.Show(message, title, buttons, icon) == MessageBoxResult.OK)
            {
                File.Copy(newImageFile, Path.Combine(selectedSavePath, newFileName));
                return true;
            }

            return false;
        }

        private void resolveCellContentTextBoxFromSavePath(string fileSavePath, string fileName)
        {
            var cellContentPath = Path.Combine(fileSavePath, fileName);
            var Context = DataContext as EditorVM;
            var uriText = Context.SelectedCsvFile.ColumnConfigurations[ColumnNr].URI;

            cellContentPath = string.IsNullOrEmpty(uriText)
                ? cellContentPath.Replace(Context.RootRepositoryPath, "").Replace("\\", "/")
                : cellContentPath.Replace(uriText + "\\", "").Replace("\\", "/");

            CellContentTextBox.Text = cellContentPath;
        }
    }
}
