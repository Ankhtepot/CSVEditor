using CSVEditor.ViewModel;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using static CSVEditor.ViewModel.ReplaceImageVM;

namespace CSVEditor.View
{
    /// <summary>
    /// Interaction logic for ReplaceImageWindow.xaml
    /// </summary>
    public partial class ReplaceImageWindow : Window
    {
        public ReplaceImageResult WindowResult = ReplaceImageResult.Canceled;
        private ReplaceImageVM Context;

        public ReplaceImageWindow()
        {
            InitializeComponent();
        }

        public void SetImagePaths(string newImagePath, string savePath, string currentImageRelativePath)
        {
            Context = DataContext as ReplaceImageVM;
            Context.SetImagePaths(newImagePath, savePath, currentImageRelativePath);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Context.OnWindowResultChange += (result) => WindowResult = result;
        }
    }
}
