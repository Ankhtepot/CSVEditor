using System.Windows;
using CSVEditor.ViewModel;
using static CSVEditor.ViewModel.ReplaceImageVM;

namespace CSVEditor.View
{
    /// <summary>
    /// Interaction logic for ReplaceImageWindow.xaml
    /// </summary>
    public partial class ReplaceImageWindow
    {
        private ReplaceImageVM Context;

        public ReplaceImageResult WindowResult = ReplaceImageResult.Canceled;
        public string NewSavePath => Context?.NewSavePath;

        public ReplaceImageWindow()
        {
            InitializeComponent();
        }

        public void SetImagePaths(string newImagePath, string savePath, string currentImagePath)
        {
            Context = DataContext as ReplaceImageVM;
            Context?.SetImagePaths(newImagePath, savePath, currentImagePath);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (Context == null) return;

            Context.OnWindowResultChange += (result) => WindowResult = result;
            Context.OnWindowCloseRequested += Close;
        }
    }
}
