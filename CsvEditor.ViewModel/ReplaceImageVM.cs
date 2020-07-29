using CSVEditor.Model.Services;
using Prism.Commands;
using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Media;

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
            get => newImagePath;
            private set 
            {
                newImagePath = value;
                OnPropertyChanged();
            }
        }

        private string currentImagePath;
        public string CurrentImagePath
        {
            get => currentImagePath;
            private set 
            {
                currentImagePath = value;
                OnPropertyChanged();
            }
        }

        private string savePath;
        public string SavePath
        {
            get => savePath;
            set 
            {
                savePath = value;
                OnPropertyChanged();
            }
        }

        private bool overwrite;
        public bool Overwrite
        {
            get => overwrite;
            set 
            {
                overwrite = value;
                OnPropertyChanged();
            }
        }

        private bool actionChecked;
        public bool ActionChecked
        {
            get => actionChecked;
            set 
            {
                actionChecked = value;
                OnPropertyChanged();
            }
        }

        private string newSavePath;
        public string NewSavePath
        {
            get => newSavePath;
            set 
            {
                newSavePath = value;
                OnPropertyChanged();
            }
        }

        public DelegateCommand CancelCommand { get; set; }
        public DelegateCommand DeleteCurrentImageAndSaveCommand { get; set; }
        public DelegateCommand OverwriteCurrentImageCommand { get; set; }
        public DelegateCommand SaveCommand { get; set; }

        public Action<ReplaceImageResult> OnWindowResultChange;
        public Action OnWindowCloseRequested;

        public ReplaceImageVM()
        {
            Overwrite = true;
            ActionChecked = false;
            NewSavePath = "no path to display";

            CancelCommand = new DelegateCommand(Cancel);
            DeleteCurrentImageAndSaveCommand = new DelegateCommand(DeleteCurrentImageAndSave);
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
            CloseWindow();
        }

        private void DeleteCurrentImageAndSave()
        {
            if (File.Exists(CurrentImagePath))
            {
                File.Delete(CurrentImagePath);
            }

            OnWindowResultChange?.Invoke(ReplaceImageResult.DeleteAndSave);
            CloseWindow();
        }

        private void Cancel()
        {
            OnWindowResultChange?.Invoke(ReplaceImageResult.Canceled);
            CloseWindow();
        }

        private void Save()
        {
            OnWindowResultChange?.Invoke(ReplaceImageResult.Save);
            CloseWindow();
        }

        private void CloseWindow()
        {
            NewImageSource = FileSystemServices.GetBitmapImageFromPath("");
            CurrentImageSource = FileSystemServices.GetBitmapImageFromPath("");
            OnWindowCloseRequested?.Invoke();
        }

        public void SetImagePaths(string newImagePath, string savePath, string cellContentPath)
        {
            NewImagePath = newImagePath;
            CurrentImagePath = Path.Combine(savePath, cellContentPath.ToSystemPath());
            SavePath = savePath;

            Overwrite = Path.GetFileName(newImagePath) == Path.GetFileName(cellContentPath);

            var lastSlash = cellContentPath.LastIndexOf("/");

            var cleanedRelativePath = cellContentPath
                .Substring(0,  lastSlash != -1 ? lastSlash : 0)
                .ToSystemPath();

            NewSavePath = Path.Combine(SavePath, cleanedRelativePath, Path.GetFileName(NewImagePath));

            NewImageSource = FileSystemServices.GetBitmapImageFromPath(newImagePath);
            CurrentImageSource = FileSystemServices.GetBitmapImageFromPath(CurrentImagePath);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
