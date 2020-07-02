using CSVEditor.ViewModel;
using System;
using System.Windows;
using System.Windows.Controls;

namespace CSVEditor.View
{
    /// <summary>
    /// Interaction logic for DateGuideWindow.xaml
    /// </summary>
    public partial class DateGuideWindow : Window
    {
        private DateGuideVM VM;
        
        public DateGuideWindow(string inputUriText)
        {
            InitializeComponent();
            VM = new DateGuideVM(inputUriText);
            TopContainer.DataContext = VM;
            VM.ChosenTime = DateTime.Now;
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            VM.InputUriText = ((ComboBox)sender).SelectedItem as string;
        }
    }
}
