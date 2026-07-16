using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using CSVEditor.Core.Services;
using CSVEditor.Core.Properties;
using Prism.Commands;

namespace CSVEditor.ViewModel
{
    public class ReplaceImageVM : INotifyPropertyChanged
    {
        public ImageSource NewImageSource
        {
            get 
            {
                if (field == null)
                {
                    return FileSystemService.GetBitmapImageFromPath("");
                }
                return field; 
            }
            private set 
            {
                field = value;
                OnPropertyChanged();
            }
        }

        public ImageSource CurrentImageSource
        {
            get 
            {
                if (field == null)
                {
                    return FileSystemService.GetBitmapImageFromPath("");
                }
                return field;
            }
            private set
            {
                field = value;
                OnPropertyChanged();
            }
        }

        public string NewImagePath
        {
            get;
            private set
            {
                field = value;
                OnPropertyChanged();
            }
        }

        public string CurrentImagePath
        {
            get;
            private set
            {
                field = value;
                OnPropertyChanged();
            }
        }

        public string SavePath
        {
            get;
            set
            {
                field = value;
                OnPropertyChanged();
            }
        }

        public bool Overwrite
        {
            get;
            set
            {
                field = value;
                OnPropertyChanged();
            }
        }

        public bool ActionChecked
        {
            get;
            set
            {
                field = value;
                OnPropertyChanged();
            }
        }

        public string NewSavePath
        {
            get;
            set
            {
                field = value;
                OnPropertyChanged();
            }
        }

        public DelegateCommand CancelCommand { get; set; }
        public DelegateCommand DeleteCurrentImageAndSaveCommand { get; set; }
        public DelegateCommand OverwriteCurrentImageCommand { get; set; }
        public DelegateCommand SaveCommand { get; set; }
        public DelegateCommand<string> SelectSavePathCommand { get; set; }

        public Action<ReplaceImageResult> OnWindowResultChange;
        public Action OnWindowCloseRequested;

        public ReplaceImageVM()
        {
            Overwrite = true;
            ActionChecked = false;
            NewSavePath = Resources.NoPathToDisplayText;

            CancelCommand = new DelegateCommand(Cancel);
            DeleteCurrentImageAndSaveCommand = new DelegateCommand(DeleteCurrentImageAndSave);
            OverwriteCurrentImageCommand = new DelegateCommand(OverwriteCurrentImage);
            SaveCommand = new DelegateCommand(Save);
            SelectSavePathCommand = new DelegateCommand<string>(SelectSavePath);
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

        private void SelectSavePath(string currentPath)
        {
            var currentFileExtension = Path.GetExtension(currentPath);
            var filter = string.IsNullOrEmpty(currentFileExtension) 
                ? null 
                : string.Format(Resources.FileFilterFormat, currentFileExtension);

            var newPath = FileSystemService.QueryUserForPath(currentPath, Resources.SelectSavePathText, filter);

            if (!string.IsNullOrEmpty(newPath))
            {
                SetImagePaths(NewImagePath, newPath, CurrentImagePath);
            }
        }

        private void CloseWindow()
        {
            NewImageSource = FileSystemService.GetBitmapImageFromPath("");
            CurrentImageSource = FileSystemService.GetBitmapImageFromPath("");
            OnWindowCloseRequested?.Invoke();
        }

        public void SetImagePaths(string newImagePath, string savePath, string currentImagePath)
        {
            NewImagePath = newImagePath;
            CurrentImagePath = currentImagePath;

            SavePath = savePath;

            // Extract relative path structure from newImagePath for currentImagePath
            // If currentImagePath has subfolders (e.g., savePath\SomeFolder\ImageName.jpg),
            // preserve them in NewSavePath
            string newSavePath;
            
            if (Path.IsPathRooted(currentImagePath) && Path.IsPathRooted(savePath))
            {
                // Both are absolute paths
                if (currentImagePath.StartsWith(savePath, StringComparison.OrdinalIgnoreCase))
                {
                    string relativePath = currentImagePath.Substring(savePath.Length).TrimStart('\\');
                    string currentDir = Path.GetDirectoryName(relativePath);
                    newSavePath = Path.Combine(SavePath, currentDir!, Path.GetFileName(NewImagePath));
                }
                else
                {
                    newSavePath = Path.Combine(SavePath, Path.GetFileName(NewImagePath));
                }
            }
            else if (!Path.IsPathRooted(currentImagePath) && !Path.IsPathRooted(savePath))
            {
                // Both are relative paths
                string currentDir = Path.GetDirectoryName(currentImagePath)?.TrimStart('\\');
                newSavePath = string.IsNullOrEmpty(currentDir) 
                    ? Path.Combine(SavePath, Path.GetFileName(NewImagePath)) 
                    : Path.Combine(SavePath, currentDir, Path.GetFileName(NewImagePath));
            }
            else
            {
                // Mixed absolute/relative - just use filename
                newSavePath = Path.Combine(SavePath, Path.GetFileName(NewImagePath));
            }

            NewSavePath = newSavePath;

            Overwrite = File.Exists(NewSavePath);

            NewImageSource = FileSystemService.GetBitmapImageFromPath(newImagePath);
            CurrentImageSource = FileSystemService.GetBitmapImageFromPath(CurrentImagePath);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
