using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using CSVEditor.Core.Extensions;
using CSVEditor.Core.HelperClasses;
using CSVEditor.Core.Services;
using CSVEditor.ViewModel;
using static CSVEditor.ViewModel.ReplaceImageVM.ReplaceImageResult;

namespace CSVEditor.View.Controls.EditGridCellElements
{
    /// <summary>
    /// Interaction logic for ImageElementControl.xaml
    /// </summary>
    public partial class ImageElementControl
    {
        private EditorVM Context;

        public static readonly string RootDirectory = CSVEditor.Core.Properties.Resources.RootDirectoryLabel;

        private static string LastAcceptedImageSavePath;
        private string CurrentImagePath;

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

            control.Context ??= control.DataContext as EditorVM;

            var configUri = control.Context?.SelectedCsvFile.ColumnConfigurations[control.ColumnNr].URI;
            var newImage = control.GetImageSource(imageCellContent, control.Context?.RootRepositoryPath, configUri);

            var cellContentBinding =
                new Binding($"SelectedCsvFile.Lines[{control.Context?.SelectedItemIndex}][{control.ColumnNr}]")
                {
                    Source = control.Context,
                    Mode = BindingMode.TwoWay,
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                };

            control.UriContentTextBlock.Text = string.IsNullOrEmpty(configUri)
                ? $"{RootDirectory}: {control.Context?.RootRepositoryPath}"
                : configUri;

            control.CellContentTextBox.SetBinding(TextBox.TextProperty, cellContentBinding);

            control.ImageFromSource.Source = newImage;
        }

        public BitmapImage GetImageSource(string cellContent, string rootRepositoryPath, string configUri)
        {
            var path = cellContent.ToSystemPath();
            var uriPath = configUri;
            CurrentImagePath = Path.Combine(string.IsNullOrEmpty(configUri) ? rootRepositoryPath : uriPath, path);

            return FileSystemService.GetBitmapImageFromPath(CurrentImagePath);
        }

        private void CellContentTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var context = DataContext as EditorVM;

            if (ColumnNr >= context?.SelectedCsvFile.ColumnCount)
            {
                return;
            }

            var uriText = context?.SelectedCsvFile.ColumnConfigurations[ColumnNr].URI;

            ImageFromSource.Source = GetImageSource(
                ((TextBox)sender).Text,
                context?.RootRepositoryPath,
                uriText);
        }

        private void ImageFromSource_PreviewDrop(object sender, DragEventArgs e)
        {
            var newImageFile = ((string[])e?.Data?.GetData(DataFormats.FileDrop))?[0];

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

            Context ??= DataContext as EditorVM;

            LastAcceptedImageSavePath ??= Context?.RootRepositoryPath;

            var uriText = Context?.SelectedCsvFile.ColumnConfigurations[ColumnNr].URI;

            var newImageFileName = Path.GetFileName(newImageFile);

            var selectedSavePath = string.IsNullOrEmpty(CellContentTextBox.Text) 
                                   || string.IsNullOrEmpty(uriText)
                                   || uriText == Context?.RootRepositoryPath
                ? FileSystemService.QueryUserForPath(LastAcceptedImageSavePath, string.Format(CSVEditor.Core.Properties.Resources.SaveNewImageSourceFileTitle, newImageFileName))
                : uriText;

            if (selectedSavePath == null || newImageFile == CurrentImagePath)
            {
                return false;
            }

            if (Path.GetDirectoryName(newImageFile) == Path.GetDirectoryName(CurrentImagePath)
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
                var replaceWindow = new ReplaceImageWindow();
                replaceWindow.SetImagePaths(newImageFile, selectedSavePath, CurrentImagePath);
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
            LastAcceptedImageSavePath = fileSavePath;
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
            var filter = CSVEditor.Core.Properties.Resources.ImageFilesFilter;
            var selectedImage = FileSystemService.QueryUserToSelectFile(path, Constants.REPLACE_IMAGE_FILE, filter);

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
