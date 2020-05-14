using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace CSVEditor.ViewModel.Commands
{
    public class CancelActiveWorkerAsyncCommand : ICommand
    {
        private EditorVM VM;

        public event EventHandler CanExecuteChanged;

        public CancelActiveWorkerAsyncCommand(EditorVM vM)
        {
            VM = vM ?? throw new ArgumentNullException(nameof(vM));
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            Console.WriteLine("Canceling Active Worker: " + VM.ActiveWorker.GetType().Name);
            VM.CancelActiveWorkerAsync();
        }
    }
}
