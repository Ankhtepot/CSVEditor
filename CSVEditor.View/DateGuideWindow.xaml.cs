using CSVEditor.Model.HelperClasses;
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
        private CellInfo OriginalCellInfo;

        public CellInfo WindowResult { get; set; }

        public DateGuideWindow(CellInfo cellInfo)
        {
            InitializeComponent();
            OriginalCellInfo = cellInfo;
            VM = new DateGuideVM(cellInfo.Content);
            TopContainer.DataContext = VM;
            VM.ChosenTime = DateTime.Now;
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            VM.InputUriText = ((ComboBox)sender).SelectedItem as string;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            WindowResult = OriginalCellInfo;
            Close();
        }

        private void AcceptButton_Click(object sender, RoutedEventArgs e)
        {
            WindowResult = new CellInfo()
            {
                Content = VM.InputUriText,
                ColumnNr = OriginalCellInfo.ColumnNr
            };

            Close();
        }
    }
}
