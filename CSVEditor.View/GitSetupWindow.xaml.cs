using CSVEditor.Model.HelperClasses;
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

namespace CSVEditor.View
{
    /// <summary>
    /// Interaction logic for GitSetupWindow.xaml
    /// </summary>
    public partial class GitSetupWindow : Window
    {
        public GitOptions GitOptions { get; set; }

        public GitSetupWindow(GitOptions gitOptions)
        {
            InitializeComponent();

            DataContext = this;

            GitOptions = gitOptions;
        }
    }
}
