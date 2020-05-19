using CSVEditor.Model;
using CSVEditor.View.Controls.ControlsViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CSVEditor.View.Controls
{
    /// <summary>
    /// Interaction logic for CsvFileGridViewerControl.xaml
    /// </summary>
    public partial class CsvFileGridViewerControl : UserControl
    {        
        private CsvFileGridViewerControlVM ControlVM;

        public CsvFile InputCsvFile
        {
            get { return (CsvFile)GetValue(InputCsvFileProperty); }
            set { SetValue(InputCsvFileProperty, value); ControlVM.LocalCsvFile = InputCsvFile; }
        }

        public static readonly DependencyProperty InputCsvFileProperty =
            DependencyProperty.Register("InputCsvFile", typeof(CsvFile), typeof(CsvFileGridViewerControl), new PropertyMetadata(CsvFileGridViewerControlVM.DEFAULT_FILE, InputCsvFileChanged));        

        private static void InputCsvFileChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (CsvFileGridViewerControl)d;
            var grid = control.OutterListView;
            var newValue = (CsvFile)e.NewValue;
            control.ControlVM.LocalCsvFile = newValue;
            control.ControlVM.TestText = control.ControlVM.LocalCsvFile?.Lines?[0][0];
            //control.OutterListView.ItemsSource = control.ControlVM.LocalCsvFile?.Lines?[0];
            var csvFile = control.ControlVM.LocalCsvFile ?? null;

            if (csvFile != null)
            {
                GridView grdView = new GridView();
                for (int i = 0; i < csvFile.HeadersStrings.Count; i++)
                {
                    GridViewColumn newColumn = new GridViewColumn();
                    newColumn.DisplayMemberBinding = new Binding(csvFile.Lines[0][i]);
                    newColumn.Header = csvFile.HeadersStrings[i];
                    newColumn.Width = 120;
                    grdView.Columns.Add(newColumn);
                }
                control.OutterListView.View = grdView;
            }
        }

        public CsvFileGridViewerControl()
        {
            InitializeComponent();
            ControlVM = new CsvFileGridViewerControlVM();
            TopContainer.DataContext = ControlVM;
        }
    }
}
