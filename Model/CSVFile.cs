using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using CSVEditor.Annotations;

namespace CSVEditor.Model
{
    public class CsvFile : INotifyPropertyChanged
    {
        private string fullName;

        public string FullName {
            get { return fullName; }
            set
            {
                fullName = value;
                OnPropertyChanged(nameof(FullName));
            }
        }

        private int headerIndex;

        public int HeaderIndex {
            get { return headerIndex; }
            set
            {
                headerIndex = value; 
                OnPropertyChanged(nameof(HeaderIndex));
            }
        }

        private string[] headersStrings;

        public string[] HeadersStrings {
            get { return headersStrings; }
            set
            {
                headersStrings = value; 
                OnPropertyChanged(nameof(HeadersStrings));
            }
        }

        private List<CsvLine> lines;

        public List<CsvLine> Lines {
            get { return lines; }
            set
            {
                lines = value; 
                OnPropertyChanged(nameof(Lines));
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
