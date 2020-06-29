using System;
using System.Collections.Generic;
using System.Text;

namespace CSVEditor.Model.Services
{
    public static class PZListExtensions
    {
        public static bool IsEqualTo<T>(this List<List<T>> sourceList, List<List<T>> otherList) where T : class
        {
            if ((sourceList == null && otherList != null)
                || (sourceList != null && otherList == null)
                || (sourceList.Count != otherList.Count)
                || (sourceList.Count > 0 && (sourceList[0].Count != otherList[0].Count)))
            {
                return false;
            }

            for (int line = 0; line < sourceList.Count; line++)
            {
                var lineCompare = CompareListLine(sourceList[line], otherList[line]);
                if (!lineCompare)
                {
                    return false;
                }
            };

            return true;
        }

        public static bool IsEqualTo<T>(this List<T> sourceList, List<T> otherList) where T : class
        {
            if ((sourceList == null && otherList != null)
                || (sourceList != null && otherList == null)
                || (sourceList.Count != otherList.Count))
            {
                return false;
            }

            return CompareListLine(sourceList, otherList);
        }

        private static bool CompareListLine<T>(List<T> sourceList, List<T> otherList) where T : class
        {
            for (int cell = 0; cell < sourceList.Count; cell++)
            {
                if (sourceList[cell] != otherList[cell])
                {
                    return false;
                }
            }

            return true;
        }
    }
}
