using Prism.Commands;
using System.Windows.Controls;

namespace CSVEditor.ViewModel
{
    public class LineNavigationBarVM
    {
        private TextBox textBox;

        public int CurrentNumber
        {
            get
            {
                int value = -1;
                int.TryParse(textBox.Text, out value);
                return value;
            }
            set
            {
                textBox.Text = value.ToString();
            }
        }

        public DelegateCommand ToBeginningCommand;

        public LineNavigationBarVM(TextBox textBox) 
        {
            this.textBox = textBox;

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
