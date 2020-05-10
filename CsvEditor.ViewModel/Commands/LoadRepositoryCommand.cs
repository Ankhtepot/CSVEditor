using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace CSVEditor.ViewModel.Commands
{
    public class LoadRepositoryCommand : ICommand
    {
        public EditorVM VM;

        public LoadRepositoryCommand(EditorVM vm)
        {
            VM = vm;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            VM.LoadRepository();
        }

        public event EventHandler CanExecuteChanged;
    }
}
