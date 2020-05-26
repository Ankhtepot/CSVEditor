using CSVEditor.ViewModel;
using System.Windows;

namespace CSVEditor.View
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class EditorWindow : Window
    {
        public EditorVM EditorVM;
        public EditorWindow()
        {
            InitializeComponent();
            EditorVM = new EditorVM();
            TopContainer.DataContext = EditorVM;
        }
    }
}
