using System;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace CSVEditor.Model.Services
{
    public class ResourceHelper
    {
        /// <summary>
        /// Author: Eric Quellet
        /// Load a resource WPF-BitmapImage (png, bmp, ...) from embedded resource defined as 'Resource' not as 'Embedded resource'.
        /// </summary>
        /// <param name="pathInApplication">Path without starting slash</param>
        /// <param name="assembly">Usually 'Assembly.GetExecutingAssembly()'. If not mentionned, I will use the calling assembly</param>
        /// <returns></returns>
        public static Uri LoadBitmapUriSourceFromResource(string pathInApplication, Assembly assembly = null)
        {
            if (assembly == null)
            {
                assembly = Assembly.GetCallingAssembly();
            }

            if (pathInApplication[0] == '/')
            {
                pathInApplication = pathInApplication.Substring(1);
            }

            return new Uri(@"pack://application:,,,/CsvEditor.View;component/" + pathInApplication, UriKind.Absolute);
        }

        public static BitmapImage GetBitmapImageFromResources(string resourcePath, int width, int height, Assembly assembly = null)
        {
            BitmapImage bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.UriSource = LoadBitmapUriSourceFromResource(resourcePath, assembly);
            bitmapImage.EndInit();
            
            return bitmapImage;
        }
    }
}
