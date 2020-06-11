﻿using JetBrains.Annotations;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using static CSVEditor.Model.Enums;

namespace CSVEditor.Model
{
    public class CsvColumnConfiguration : INotifyPropertyChanged
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

        private string uRI;

        public string URI {
            get { return uRI; }
            set
            {
                uRI = value;
                OnPropertyChanged(nameof(URI));
            }
        }

        public CsvColumnConfiguration() : this (FieldType.TextBox, "")
        {
        }

        public CsvColumnConfiguration(FieldType type, string uRI = "")
        {
            Type = type;
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
