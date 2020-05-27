using CSVEditor.Model;
using CSVEditor.Services;
using CSVEditor.ViewModel.Abstracts;
using JetBrains.Annotations;
using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using static CSVEditor.Model.Enums;

namespace CSVEditor.ViewModel
{
    public class AsyncVM : INotifyPropertyChanged
    {
        private const int UPDATE_PROGRESS_DELAY = 100;

        private string selectedFileRaw;
        public string SelectedFileRaw
        {
            get { return selectedFileRaw; }
            set { selectedFileRaw = value; OnPropertyChanged(); }
        }

        private bool isConverterProcessing;
        public bool IsConverterProcessing
        {
            get { return isConverterProcessing; }
            set { isConverterProcessing = value; OnPropertyChanged(); }
        }

        private WorkStatus workingStatus;

        public WorkStatus WorkingStatus
        {
            get { return workingStatus; }
            set
            {
                workingStatus = value;
                Console.WriteLine($"EditorVM:Working status = {value}");
                OnPropertyChanged(nameof(WorkingStatus));
            }
        }

        private int workProgress;

        public int WorkProgress
        {
            get { return workProgress; }
            set
            {
                workProgress = value;
                OnPropertyChanged();
            }
        }

        private AbstractEditorVMWorker activeWorker;

        public AbstractEditorVMWorker ActiveWorker //this field is being handled by AbstractEditorVMWorker
        {
            get { return activeWorker; }
            set { activeWorker = value; OnPropertyChanged(); }
        }


        private EditorVM editorVM;

        public EditorVM EditorVM
        {
            get { return editorVM; }
            set { editorVM = value; OnPropertyChanged(); }
        }


        public AsyncVM(EditorVM editorVM)
        {
            EditorVM = editorVM;
            IsConverterProcessing = false;
            SelectedFileRaw = Constants.NO_FILE_SELECTED;
        }

        public async void SetRawTextFromAbsPath(string path)
        {
            var cts = new CancellationTokenSource();

            UpdateProcessingFileTextTask(cts.Token);
            string rawText = await Task.Run(() => GetRawTextTask(cts, path));
            await Task.Delay(UPDATE_PROGRESS_DELAY * 3);
            SelectedFileRaw = rawText;
        }

        private string GetRawTextTask(CancellationTokenSource cts, string path)
        {
            var result = Task.Run( async () =>
            {
                var rawText = await Task.Run(() => FileProcessingServices.GetRawFileText(path));
                cts.Cancel();
                return rawText;
            });

            return result.Result;
        }

        private void UpdateProcessingFileTextTask(CancellationToken token)
        {
            Task.Run( async () =>
            {
                SelectedFileRaw = Constants.PROCESSING_FILE;
                while (!token.IsCancellationRequested)
                {
                    await Task.Delay(UPDATE_PROGRESS_DELAY);
                    var dotsCount = SelectedFileRaw.Count<char>(ch => ch == '.');
                    //TODO: check and fix updating after main task finishes if its broken
                    SelectedFileRaw = dotsCount < 10 ? SelectedFileRaw + "." : SelectedFileRaw.Replace(".", ""); 
                    Console.WriteLine("Updated Progress Text: " + SelectedFileRaw);
                }
            });
        }

        public void CancelActiveWorkerAsync()
        {
            ActiveWorker?.CancelAsync();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
