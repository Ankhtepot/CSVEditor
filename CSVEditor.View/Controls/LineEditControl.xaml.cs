using CSVEditor.ViewModel;
using System;
using System.Windows;
using System.Windows.Controls;
using CSVEditor.Core.HelperClasses;

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
            get => (int)GetValue(SelectedLineIndexProperty);
            set => SetValue(SelectedLineIndexProperty, value);
        }
        public static readonly DependencyProperty SelectedLineIndexProperty =
            DependencyProperty.Register(nameof(SelectedLineIndex), typeof(int), typeof(LineEditControl), new PropertyMetadata(0, SelectedLineIndexChanged));

        public LineEditControl()
        {
            InitializeComponent();
        }

        private static void SelectedLineIndexChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            LineEditControl control = (LineEditControl)d;
            control.TopContainer.Children.Clear();

            if (!control.IsVisible)
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
            EditorVM context = topContainer.DataContext as EditorVM;
            topContainer.Children.Clear();

            VM = new EditGridControlViewModel(
               resources,
               context);

            
            Logger.LogInfo($@"LineEditControl, building new Grid for Index =  {context?.SelectedItemIndex}.");
            
            topContainer.Children.Add(VM.GetEditLinesGridForNewCsvFile());
        }
    }
}
