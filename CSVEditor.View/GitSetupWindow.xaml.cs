using CSVEditor.Model.HelperClasses;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace CSVEditor.View
{
    /// <summary>
    /// Interaction logic for GitSetupWindow.xaml
    /// </summary>
    public partial class GitSetupWindow : Window, INotifyPropertyChanged
    {
        private GitOptions gitOptions;

        public GitOptions GitOptions
        {
            get { return gitOptions; }
            set 
            {
                gitOptions = value;
                OnPropertyChanged();
            }
        }


        public bool Canceled { get; set; }

        public GitSetupWindow(GitOptions gitOptions)
        {
            InitializeComponent();

            DataContext = this;
            Canceled = true;
            GitOptions = gitOptions;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            Canceled = false;
            Close();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
