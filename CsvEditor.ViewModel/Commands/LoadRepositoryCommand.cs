using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using static CSVEditor.Model.Enums;

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
            if(parameter == null)
            {
                return true;
            }

            var status = (WorkStatus)parameter;
            return status == WorkStatus.Idle;
        }

        public void Execute(object parameter)
        {
            VM.LoadRepository();
        }

        public event EventHandler CanExecuteChanged;
    }
}
