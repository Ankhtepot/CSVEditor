using System;
using System.Windows.Media.Imaging;

namespace CSVEditor.Core.Services
{
    public static class ResourceHelper
    {
        /// <summary>
        /// Author: Eric Quellet
        /// Load a resource WPF-BitmapImage (png, bmp, ...) from the embedded resource defined as 'Resource' not as 'Embedded resource'.
        /// </summary>
        /// <param name="pathInApplication">Path without starting slash</param>
        /// <returns></returns>
        public static Uri LoadBitmapUriSourceFromResource(string pathInApplication)
        {
            if (pathInApplication[0] == '/')
            {
                pathInApplication = pathInApplication.Substring(1);
            }

            // legacy, left for posterity: return new Uri(@"pack://application:,,,/CsvEditor.View;component/" + pathInApplication, UriKind.Absolute);
            return new Uri(
                $"pack://application:,,,/{pathInApplication}",
                UriKind.Absolute);
        }

        public static BitmapImage GetBitmapImageFromResources(string resourcePath)
        {
            BitmapImage bitmapImage = new();
            bitmapImage.BeginInit();
            bitmapImage.UriSource = LoadBitmapUriSourceFromResource(resourcePath);
            bitmapImage.EndInit();
            
            return bitmapImage;
        }
    }
}
