using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;

namespace CSVEditor.View.Controls
{
    public class LineNavigationBarControlViewModel
    {
        private LineNavigationBarControl control;

        public int CurrentNumber
        {
            get
            {
                int value = -1;
                int.TryParse(control.NumberTextBox.Text, out value);
                return value + 1;
            }
            set
            {
                control.NumberTextBox.Text = value.ToString();
            }
        }

        public DelegateCommand ToBeginningCommand { get; set; }

        public LineNavigationBarControlViewModel(LineNavigationBarControl control) 
        {
            this.control = control;

            ToBeginningCommand = new DelegateCommand(ToBeginning, ToBeginnig_canExecute);
        }

        private bool ToBeginnig_canExecute()
        {
            return CurrentNumber > 0;
        }

        private void ToBeginning()
        {
            CurrentNumber = 0;
        }
    }
}
