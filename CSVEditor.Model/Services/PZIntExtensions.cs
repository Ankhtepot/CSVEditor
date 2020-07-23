namespace CSVEditor.Model.Services
{
    public static class PZIntExtensions
    {
        public static int Clamp(this int source, int minValue, int maxValue)
        {
            if (source < minValue)
            {
                return minValue;
            }

            if (source > maxValue)
            {
                return maxValue;
            }

            return source;
        }
    }
}
