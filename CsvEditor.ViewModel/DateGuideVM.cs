using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

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

        private DateTime chosenTime;
        public DateTime ChosenTime
        {
            get
            {
                return chosenTime == null ? DateTime.Now : chosenTime; 
            }
            set { chosenTime = value; }
        }

        public List<string> DateFormatExamples
        {
            get => GetDateFormatExamples();
        }

        public string FormatedChosenTimeText
        {
            get => BuildResultText();
        }        

        public DateGuideVM(string inputUriText)
        {
            InputUriText = inputUriText ?? throw new ArgumentNullException(nameof(inputUriText));
        }

        private string BuildResultText()
        {
            return $"Date: {ChosenTime.ToShortDateString()} with date format of \"{InputUriText}\" results to: {ChosenTime.ToString(InputUriText)}.";
        }

        private List<string> GetDateFormatExamples()
        {
            return new List<string>() {
                "MM/dd/yyyy",
                "dddd, dd MMMM yyyy",
                "dddd, dd MMMM yyyy HH:mm:ss",
                "MM/dd/yyyy HH:mm",
                "MM/dd/yyyy hh:mm tt",
                "MM/dd/yyyy H:mm",
                "MM/dd/yyyy h:mm tt",
                "MM/dd/yyyy HH:mm:ss",
                "MMMM dd",
                "yyyy’-‘MM’-‘dd’T’HH’:’mm’:’ss.fffffffK",
                "ddd, dd MMM yyy HH’:’mm’:’ss ‘GMT’",
                "yyyy’-‘MM’-‘dd’T’HH’:’mm’:’ss",
                "HH:mm",
                "hh:mm tt",
                "H:mm",
                "h:mm tt",
                "HH:mm:ss",
                "yyyy MMMM",
            };
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
