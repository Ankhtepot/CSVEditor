using CSVEditor.Model.HelperClasses;
using CSVEditor.Model.Services;
using CSVEditor.Services;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace CSVEditor.ViewModel
{
    public class ReplaceImageVM : INotifyPropertyChanged
    {
        private ImageSource newImageSource;
        public ImageSource NewImageSource
        {
            get 
            {
                if (newImageSource == null)
                {
                    return FileSystemServices.GetBitmapImageFromPath("");
                }
                return newImageSource; 
            }
            private set 
            {
                newImageSource = value;
                OnPropertyChanged();
            }
        }

        private ImageSource currentImageSource;
        public ImageSource CurrentImageSource
        {
            get 
            {
                if (currentImageSource == null)
                {
                    return FileSystemServices.GetBitmapImageFromPath("");
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

        private string savePath;
        public string SavePath
        {
            get { return savePath; }
            set 
            {
                savePath = value;
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

        private bool actionChecked;
        public bool ActionChecked
        {
            get { return actionChecked; }
            set 
            {
                actionChecked = value;
                OnPropertyChanged();
            }
        }

        private string newSavePath;
        public string NewSavePath
        {
            get { return newSavePath; }
            set 
            {
                newSavePath = value;
                OnPropertyChanged();
            }
        }

        public DelegateCommand CancelCommand { get; set; }
        public DelegateCommand DeleteCurentImageAndSaveCommand { get; set; }
        public DelegateCommand OverwriteCurrentImageCommand { get; set; }
        public DelegateCommand SaveCommand { get; set; }

        public Action<ReplaceImageResult> OnWindowResultChange;

        public ReplaceImageVM()
        {
            Overwrite = true;
            ActionChecked = false;
            NewSavePath = "no path to display";

            CancelCommand = new DelegateCommand(Cancel);
            DeleteCurentImageAndSaveCommand = new DelegateCommand(DeleteCurentImageAndSave);
            OverwriteCurrentImageCommand = new DelegateCommand(OverwriteCurrentImage);
            SaveCommand = new DelegateCommand(Save);
        }

        public enum ReplaceImageResult
        {
            Canceled,
            Overwrite,
            DeleteAndSave,
            Save
        }

        private void OverwriteCurrentImage()
        {
            OnWindowResultChange?.Invoke(ReplaceImageResult.Overwrite);
        }

        private void DeleteCurentImageAndSave()
        {
            OnWindowResultChange?.Invoke(ReplaceImageResult.DeleteAndSave);
        }

        private void Cancel()
        {
            OnWindowResultChange?.Invoke(ReplaceImageResult.Canceled);
        }

        private void Save()
        {
            OnWindowResultChange?.Invoke(ReplaceImageResult.Save);
        }

        public void SetImagePaths(string newImagePath, string savePath, string currentImageRelativePath)
        {
            Overwrite = Path.GetFileName(newImagePath) == Path.GetFileName(currentImageRelativePath);
            NewImagePath = newImagePath;
            CurrentImagePath = currentImageRelativePath;
            SavePath = savePath;
            NewSavePath = Path.Combine(SavePath, Path.GetFileName(NewImagePath));

            NewImageSource = FileSystemServices.GetBitmapImageFromPath(newImagePath);
            CurrentImageSource = FileSystemServices.GetBitmapImageFromPath(Path.Combine(savePath, currentImageRelativePath));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
