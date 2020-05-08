using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using CSVEditor.Annotations;
using static CSVEditor.Model.Enums;

namespace CSVEditor.Model
{
    public class CsvField : INotifyPropertyChanged
    {
        private Enums.FieldType type;

        public Enums.FieldType Type {
            get { return type; }
            set
            {
                type = value;
                OnPropertyChanged(nameof(Type));
            }
        }

        private string content;

        public string Content {
            get { return content; }
            set
            {
                content = value;
                OnPropertyChanged(nameof(Content));
            }
        }

        private string uRI;

        public string URI {
            get { return uRI; }
            set
            {
                uRI = value;
                OnPropertyChanged(nameof(URI));
            }
        }

        public CsvField() : this (FieldType.TextBlock, "", "")
        {
        }

        public CsvField(FieldType type, string content, string uRI = "")
        {
            Type = type;
            Content = content ?? throw new ArgumentNullException(nameof(content));
            URI = uRI;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
