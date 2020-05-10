using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace CSVEditor.ViewModel.Converters
{
    public interface IValueConverterStandard
    {
        object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture);
    }

    public interface IMultiValueConverterStandard
    {
        object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture);
    }


    internal class NamedObject
    {
        private string _name;

        public NamedObject(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(name);
            }
            _name = name;
        }

        public override string ToString()
        {
            if (_name[0] != '{')
            {
                _name = string.Format(CultureInfo.InvariantCulture, "{{{0}}}", new object[1]
                {
            _name
                });
            }
            return _name;
        }
    }

    public static class DataBinding
    {
        public static readonly object DoNothing = new NamedObject("Binding.DoNothing");
        public static readonly object UnsetValue = new NamedObject("DependencyProperty.UnsetValue");
        public static readonly object[] DoNothingArr = new object[] { DoNothing };
    }
}
