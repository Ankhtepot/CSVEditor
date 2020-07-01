using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace CSVEditor.ViewModel
{
    public class DateGuideVM : INotifyPropertyChanged
    {
        private string inputUriText;

        public string InputUriText
        {
            get { return inputUriText; }
            set 
            {
                inputUriText = value;
                OnPropertyChanged();
            }
        }

        public DateGuideVM(string inputUriText)
        {
            InputUriText = inputUriText ?? throw new ArgumentNullException(nameof(inputUriText));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
