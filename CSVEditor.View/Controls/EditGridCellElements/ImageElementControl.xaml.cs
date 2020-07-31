﻿using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using CSVEditor.Model.HelperClasses;
using CSVEditor.Model.Services;
using CSVEditor.ViewModel;
using static CSVEditor.ViewModel.ReplaceImageVM.ReplaceImageResult;

namespace CSVEditor.View.Controls.EditGridCellElements
{
    /// <summary>
    /// Interaction logic for ImageElementControl.xaml
    /// </summary>
    public partial class ImageElementControl : UserControl
    {
        private static EditorVM Context;

        public static readonly string ROOT_DIRECTORY = "Root Directory";

        private static string LastAcceptedImageSavePath;

        public string ImageCellContent
        {
            get => (string)GetValue(ImageCellContentProperty);
            set => SetValue(ImageCellContentProperty, value);
        }
        public static readonly DependencyProperty ImageCellContentProperty =
            DependencyProperty.Register("ImageCellContent", typeof(string), typeof(ImageElementControl), new PropertyMetadata(null, ImageSourceChanged));

        public int ColumnNr
        {
            get => (int)GetValue(ColumnNrProperty);
            set => SetValue(ColumnNrProperty, value);
        }
        public static readonly DependencyProperty ColumnNrProperty =
            DependencyProperty.Register("ColumnNr", typeof(int), typeof(ImageElementControl), new PropertyMetadata(0));

        public ImageElementControl()
        {
            InitializeComponent();
            Context = DataContext as EditorVM;
        }

        private static void ImageSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (ImageElementControl)d;
            var imageCellContent = (string)e.NewValue;

            if (control == null || imageCellContent == null)
            {
                return;
            }

            Context ??= control.DataContext as EditorVM;

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
            string path = cellContent.ToSystemPath();
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
            if (!FileSystemServices.IsImageFile(newImageFile))
            {
                return true;
            }

            Context ??= DataContext as EditorVM;

            LastAcceptedImageSavePath ??= LastAcceptedImageSavePath = Context?.RootRepositoryPath;
                
            var uriText = Context?.SelectedCsvFile.ColumnConfigurations[ColumnNr].URI;

            var newImageFileName = Path.GetFileName(newImageFile);

            var selectedSavePath = string.IsNullOrEmpty(CellContentTextBox.Text) || uriText == Context.RootRepositoryPath
                ? FileSystemServices.QueryUserForPath(LastAcceptedImageSavePath, $"Save {newImageFileName} File to:")
                : uriText;

            if (selectedSavePath == null || newImageFile == Path.Combine(selectedSavePath, Path.GetFileName(newImageFile)))
            {
                return false;
            }

            if (string.IsNullOrEmpty(CellContentTextBox.Text))
            {
                if (FileSystemServices.SaveImageFile(newImageFile, selectedSavePath))
                {
                    ResolveCellContentTextBoxFromSavePath(selectedSavePath, newImageFileName);
                }
            }
            else
            {
                var replaceWindow = new ReplaceImageWindow();
                replaceWindow.SetImagePaths(newImageFile, selectedSavePath, CellContentTextBox.Text);
                replaceWindow.ShowDialog();
                selectedSavePath = replaceWindow.NewSavePath;
                if (replaceWindow.WindowResult != Canceled)
                {
                    ResolveReplaceDialogResult(newImageFile, selectedSavePath, replaceWindow.WindowResult); 
                }
            }

            return true;
        }

        private void ResolveReplaceDialogResult(string newImageFile, string selectedSavePath, ReplaceImageVM.ReplaceImageResult replaceWindowWindowResult)
        {
            if (newImageFile != selectedSavePath)
            {
                switch (replaceWindowWindowResult)
                {
                    case Overwrite:
                        {
                            if (File.Exists(selectedSavePath))
                            {
                                File.Delete(selectedSavePath);
                            }

                            File.Copy(newImageFile, selectedSavePath);
                        }; break;
                } 
            }

            ResolveCellContentTextBoxFromSavePath(selectedSavePath, Path.GetFileName(newImageFile));
        }

        private void ResolveCellContentTextBoxFromSavePath(string fileSavePath, string fileName)
        {
            var cellContentPath = Path.Combine(fileSavePath, fileName);
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
            var path = Path.Combine(Context.RootRepositoryPath, CellContentTextBox.Text.ToSystemPath());
            path = Path.GetDirectoryName(path);
            var filter = Properties.Resources.ImageFilesFilter;
            var selectedImage = FileSystemServices.QueryUserToSelectFile(path, Constants.REPLACE_IMAGE_FILE, filter);

            if (string.IsNullOrEmpty(selectedImage))
            {
                return;
            }

            if (path == Path.GetDirectoryName(selectedImage))
            {
                ResolveCellContentTextBoxFromSavePath(path, Path.GetFileName(selectedImage));
                return;
            }

            SaveNewImageSourceFile(selectedImage);
        }

        private void CellContentTextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            Context.IsFileEdited = true;
        }

        private void ImageElementControl_OnLoaded(object sender, RoutedEventArgs e)
        {
            
        }
    }
}
