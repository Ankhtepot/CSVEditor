using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using CSVEditor.Core;
using CSVEditor.Core.Extensions;
using CSVEditor.Core.HelperClasses;
using CSVEditor.Core.Services;
using CSVEditor.ViewModel;
using T = CSVEditor.Core.Properties.Resources;
using static CSVEditor.ViewModel.ReplaceImageVM.ReplaceImageResult;

namespace CSVEditor.View.Controls.EditGridCellElements
{
    /// <summary>
    /// Interaction logic for ImageElementControl.xaml
    /// </summary>
    public partial class ImageElementControl
    {
        private EditorVM _context;

        private static string LastAcceptedImageSavePath => AppOptionsService.AppOptions.LastSavedDirectoryPath;
        private static AppOptions AppOptionsPath => AppOptionsService.AppOptions;
        private string _currentImagePath;

        public static readonly DependencyProperty ImageCellContentProperty =
            DependencyProperty.Register("ImageCellContent", typeof(string), typeof(ImageElementControl), new PropertyMetadata(null, ImageSourceChanged));

        public int ColumnNr
        {
            get => (int)GetValue(ColumnNrProperty);
            set => SetValue(ColumnNrProperty, value);
        }
        public static readonly DependencyProperty ColumnNrProperty =
            DependencyProperty.Register(nameof(ColumnNr), typeof(int), typeof(ImageElementControl), new PropertyMetadata(0));

        public ImageElementControl()
        {
            InitializeComponent();
            _context = DataContext as EditorVM;
        }

        private static void ImageSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ImageElementControl control = (ImageElementControl)d;
            string imageCellContent = (string)e.NewValue;

            if (control == null || imageCellContent == null)
            {
                return;
            }

            control._context ??= control.DataContext as EditorVM;

            string configUri = control._context?.SelectedCsvFile.ColumnConfigurations[control.ColumnNr].URI;
            BitmapImage newImage = control.GetImageSource(imageCellContent, control._context?.RootRepositoryPath, configUri);

            Binding cellContentBinding =
                new($"SelectedCsvFile.Lines[{control._context?.SelectedItemIndex}][{control.ColumnNr}]")
                {
                    Source = control._context,
                    Mode = BindingMode.TwoWay,
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                };

            control.UriContentTextBlock.Text = string.IsNullOrEmpty(configUri)
                ? $"{T.RootDirectoryLabel}: {control._context?.RootRepositoryPath}"
                : configUri;

            control.CellContentTextBox.SetBinding(TextBox.TextProperty, cellContentBinding);

            control.ImageFromSource.Source = newImage;
        }

        public BitmapImage GetImageSource(string cellContent, string rootRepositoryPath, string configUri)
        {
            string path = cellContent.ToSystemPath();
            string imageRootPath = ResolveConfiguredImageDirectory(configUri);
            if (string.IsNullOrEmpty(imageRootPath))
            {
                imageRootPath = rootRepositoryPath ?? string.Empty;
            }
            _currentImagePath = Path.Combine(imageRootPath, path);

            return FileSystemService.GetBitmapImageFromPath(_currentImagePath);
        }

        private void CellContentTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            EditorVM context = DataContext as EditorVM;

            if (ColumnNr >= context?.SelectedCsvFile.ColumnCount)
            {
                return;
            }

            string uriText = context?.SelectedCsvFile.ColumnConfigurations[ColumnNr].URI;

            ImageFromSource.Source = GetImageSource(
                ((TextBox)sender).Text,
                context?.RootRepositoryPath,
                uriText);
        }

        private void ImageFromSource_PreviewDrop(object sender, DragEventArgs e)
        {
            string newImageFile = ((string[])e?.Data?.GetData(DataFormats.FileDrop))?[0];

            if (e != null)
            {
                e.Handled = SaveNewImageSourceFile(newImageFile);
            }
        }

        private bool SaveNewImageSourceFile(string newImageFile)
        {
            if (!FileSystemService.IsImageFile(newImageFile))
            {
                return true;
            }

            _context ??= DataContext as EditorVM;

            string uriText = _context?.SelectedCsvFile.ColumnConfigurations[ColumnNr].URI;
            string newImageFileName = Path.GetFileName(newImageFile);
            string configuredUriPath = ResolveConfiguredImageDirectory(uriText);
            string initialSavePath = Directory.Exists(configuredUriPath)
                ? configuredUriPath
                : LastAcceptedImageSavePath;
            string selectedSavePath = FileSystemService.QueryUserForPath(
                initialSavePath,
                string.Format(T.SaveNewImageSourceFileTitle, newImageFileName),
                dialogPathType: Enums.DialogPathType.Save);

            if (selectedSavePath == null || newImageFile == _currentImagePath)
            {
                return false;
            }

            if (Path.GetDirectoryName(newImageFile) == Path.GetDirectoryName(_currentImagePath)
                && File.Exists(newImageFile))
            {
                ResolveCellContentTextBoxFromSavePath(selectedSavePath, newImageFileName);
            }
            else if (string.IsNullOrEmpty(CellContentTextBox.Text))
            {
                if (FileSystemService.SaveImageFile(newImageFile, selectedSavePath))
                {
                    ResolveCellContentTextBoxFromSavePath(selectedSavePath, newImageFileName);
                }
            }
            else
            {
                ReplaceImageWindow replaceWindow = new();
                replaceWindow.SetImagePaths(newImageFile, selectedSavePath, _currentImagePath);
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
            if (newImageFile != selectedSavePath && File.Exists(newImageFile))
            {
                // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
                switch (replaceWindowWindowResult)
                {
                    case Overwrite:
                        File.Delete(selectedSavePath);

                        File.Copy(newImageFile, selectedSavePath);break;
                    case Save: File.Copy(newImageFile, selectedSavePath); break;
                    case DeleteAndSave:
                        File.Copy(newImageFile, selectedSavePath); break;
                }
            }

            ResolveCellContentTextBoxFromSavePath(Path.GetDirectoryName(selectedSavePath), Path.GetFileName(newImageFile));
        }

        private void ResolveCellContentTextBoxFromSavePath(string fileSavePath, string fileName)
        {
            string cellContentPath = Path.Combine(fileSavePath, fileName);
            string uriText = _context.SelectedCsvFile.ColumnConfigurations[ColumnNr].URI;
            string sourceRootPath = string.IsNullOrEmpty(uriText)
                ? _context.RootRepositoryPath
                : ResolveConfiguredImageDirectory(uriText);

            cellContentPath = Path.GetRelativePath(sourceRootPath ?? string.Empty, cellContentPath).Replace("\\", "/");

            CellContentTextBox.Text = cellContentPath;
            _context.IsFileEdited = true;
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
            string openingFolder = AppOptionsPath.LastOpenedDirectoryPath;
            openingFolder = Directory.Exists(openingFolder) 
                ? openingFolder 
                : Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
            string filter = T.ImageFilesFilter;
            string selectedImage = FileSystemService.QueryUserToSelectImageFile(openingFolder, Constants.REPLACE_IMAGE_FILE, filter);

            if (string.IsNullOrEmpty(selectedImage))
            {
                return;
            }

            SaveNewImageSourceFile(selectedImage);
        }

        private string ResolveConfiguredImageDirectory(string uriText)
        {
            string rootRepositoryPath = _context?.RootRepositoryPath;

            if (string.IsNullOrEmpty(uriText))
            {
                return rootRepositoryPath;
            }

            string uriPath = uriText.ToSystemPath();
            return Path.IsPathRooted(uriPath)
                ? uriPath
                : Path.Combine(rootRepositoryPath ?? "", uriPath);
        }

        private void CellContentTextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            _context.IsFileEdited = true;
        }

        private void ImageElementControl_OnLoaded(object sender, RoutedEventArgs e)
        {

        }
    }
}
