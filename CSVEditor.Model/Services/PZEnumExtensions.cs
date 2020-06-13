using System;
using System.Collections.Generic;
using System.Text;

namespace CSVEditor.Model.Services
{
    public static class PZEnumExtensions
    {
        public static int GetValueFromName<T>(T source,T value)
        {
            return (int)Enum.Parse(typeof(T), value.ToString());
        }

    }
}
