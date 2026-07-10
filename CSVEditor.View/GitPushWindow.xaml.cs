using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using CSVEditor.Core.HelperClasses;
using CSVEditor.ViewModel;

namespace CSVEditor.View;

public partial class GitPushWindow : Window
{
    public GitOptions GitOptions { get; set; }
    public string GitStatus { get; set; }

    public bool Canceled { get; set; }

    public GitPushWindow(GitOptions gitOptions)
    {
        InitializeComponent();

        DataContext = this;

        GitOptions = gitOptions;
        
        Canceled = true;

        GitStatus = GitVM.GetGitStatus();
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private void PushButton_OnClick(object sender, RoutedEventArgs e)
    {
        Canceled = false;
        Close();
    }

    private void CancelButton_OnClick(object sender, RoutedEventArgs e) => Close();
}