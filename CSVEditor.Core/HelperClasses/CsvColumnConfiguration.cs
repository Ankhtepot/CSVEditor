using System.ComponentModel;
using System.Runtime.CompilerServices;
using static CSVEditor.Core.HelperClasses.Enums;

namespace CSVEditor.Core.HelperClasses
{
    public class CsvColumnConfiguration : INotifyPropertyChanged
    {
        private FieldType type;
        public FieldType Type {
            get => type;
            set
            {
                type = value;
                OnPropertyChanged(nameof(Type));
            }
        }

        private string uRI;
        public string URI {
            get => uRI;
            set
            {
                uRI = value;
                OnPropertyChanged(nameof(URI));
            }
        }

        public CsvColumnConfiguration() : this (FieldType.TextBox) {}

        public CsvColumnConfiguration(FieldType type, string uRI = "")
        {
            Type = type;
            URI = uRI;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
