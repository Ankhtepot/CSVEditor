using CSVEditor.Model.HelperClasses;
using CSVEditor.Model.Services;
using CSVEditor.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Media;

namespace CSVEditor.ViewModel
{
    public class ReplaceImageVM : INotifyPropertyChanged
    {
        private Uri newImageSource;
        public Uri NewImageSource
        {
            get 
            {
                if (newImageSource == null)
                {
                    return ResourceHelper.LoadBitmapUriSourceFromResource(Constants.IMAGE_NOT_AVAIABLE_APP_PATH);
                }
                return newImageSource; 
            }
            private set 
            {
                newImageSource = value;
                OnPropertyChanged();
            }
        }

        private Uri currentImageSource;
        public Uri CurrentImageSource
        {
            get 
            {
                if (currentImageSource == null)
                {
                    return ResourceHelper.LoadBitmapUriSourceFromResource(Constants.IMAGE_NOT_AVAIABLE_APP_PATH);
                }
                return currentImageSource;
            }
            private set
            {
                currentImageSource = value;
                OnPropertyChanged();
            }
        }

        private string newImagePath;
        public string NewImagePath
        {
            get { return newImagePath; }
            private set 
            {
                newImagePath = value;
                OnPropertyChanged();
            }
        }

        private string currentImagePath;
        public string CurrentImagePath
        {
            get { return currentImagePath; }
            private set 
            {
                currentImagePath = value;
                OnPropertyChanged();
            }
        }

        private bool overwrite;
        public bool Overwrite
        {
            get { return overwrite; }
            set 
            {
                overwrite = value;
                OnPropertyChanged();
            }
        }

        public ReplaceImageVM()
        {
            Overwrite = true;
        }

        public void SetImagePaths(string newImagePath, string savePath, string currentImagePath)
        {
            Overwrite = Path.GetFileName(newImagePath) == Path.GetFileName(currentImagePath);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
