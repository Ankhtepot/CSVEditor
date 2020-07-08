using CSVEditor.Model.HelperClasses;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace CSVEditor.ViewModel
{
    public class SaveVM : INotifyPropertyChanged
    {
        private SaveOptions saveOptions;    

        public SaveOptions SaveOptions
        {
            get { return saveOptions; }
            set 
            {
                saveOptions = value;
                OnPropertyChanged();
            }
        }


        public SaveVM()
        {
           
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
