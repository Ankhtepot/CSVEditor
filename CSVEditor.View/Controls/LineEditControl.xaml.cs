using CSVEditor.Model;
using CSVEditor.Model.Services;
using CSVEditor.ViewModel;
using System;
using System.Windows;
using System.Windows.Controls;

namespace CSVEditor.View.Controls
{
    /// <summary>
    /// Interaction logic for LineEditControler.xaml
    /// </summary>
    public partial class LineEditControl : UserControl
    {
        private static EditGridControlViewModel VM;

        public int SelectedLineIndex
        {
            get { return (int)GetValue(SelectedLineIndexProperty); }
            set { SetValue(SelectedLineIndexProperty, value); }
        }
        public static readonly DependencyProperty SelectedLineIndexProperty =
            DependencyProperty.Register("SelectedLineIndex", typeof(int), typeof(LineEditControl), new PropertyMetadata(0, SelectedLineIndexChanged));

        public LineEditControl()
        {
            InitializeComponent();
        }

        private static void SelectedLineIndexChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (LineEditControl)d;
            control.TopContainer.Children.Clear();

            if (control == null || !control.IsVisible)
            {
                return;
            }

            BuildGrid(control.TopContainer, control.Resources);
        }

        private void TopContainer_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            TopContainer.Children.Clear();

            if ((bool)e.NewValue == true)
            {
                BuildGrid(TopContainer, Resources);
            }
        }

        private static void BuildGrid(Grid topContainer, ResourceDictionary resources)
        {
            var Context = topContainer.DataContext as EditorVM;
            topContainer.Children.Clear();

            VM = new EditGridControlViewModel(
               resources,
               Context);

            Console.WriteLine($"LineEditControl, building new Grid for Index =  {Context.SelectedItemIndex}.");
            
            topContainer.Children.Add(VM.GetEditLinesGridForNewCsvFile());
        }
    }
}
