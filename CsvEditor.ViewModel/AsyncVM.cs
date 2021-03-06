﻿using CSVEditor.Model;
using CSVEditor.Model.HelperClasses;
using CSVEditor.ViewModel.Abstracts;
using CSVEditor.ViewModel.BackgroundWorkers;
using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CSVEditor.Model.Services;
using static CSVEditor.Model.HelperClasses.Enums;

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
                OnPropertyChanged();
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
                    SelectedFileRaw = dotsCount < 10 ? SelectedFileRaw + "." : SelectedFileRaw.Replace(".", ""); 
                }
            });
        }

        public void LoadRepository()
        {
            EditorVM.SelectedFile = null;
            EditorVM.SelectedCsvFile = null;

            EditorVM.RootRepositoryPath = FileSystemServices.QueryUserForRootRepositoryPath(Constants.SELECT_PROJECT_ROOT_DIRECTORY);

            EditorVM.GitVM.SetGitInfo(EditorVM.RootRepositoryPath);

            EditorVM.CsvFilesStructure.Clear();

            new LoadDirectoriesWithCsvWorker(EditorVM).RunAsync(EditorVM.RootRepositoryPath);            
        }

        public bool LoadRepository_CanExecute()
        {
            return WorkingStatus != WorkStatus.Working;
        }

        public void CancelActiveWorkerAsync()
        {
            ActiveWorker?.CancelAsync();
        }

        public async Task<string> CsvFileToTextTask(CsvFile csvFile)
        {
            WorkingStatus = WorkStatus.Working;

            if (csvFile.Lines.Count < 1)
            {
                WorkingStatus = WorkStatus.Done;
            }

            var progressStep = 100d / csvFile.Lines.Count;
            WorkProgress = 0;

            var progress = new Progress<int>(lineNr =>
            {
                WorkProgress = (int)((lineNr + 1) * progressStep); // +1 for headers line
            });

            var csvText = await Task.Run(() => ConvertCsvFileToTextTask(csvFile, progress));

            WorkingStatus = WorkStatus.Done;

            return csvText;
        }

        private string ConvertCsvFileToTextTask(CsvFile csvFile, IProgress<int> progress)
        {
            return Task.Run(async () =>
            {
                var stringBuilder = new StringBuilder(FileProcessingServices.HeadersLineToStringLine(csvFile));

                for (int i = 0; i < csvFile.Lines.Count; i++)
                {
                    var processedLine = await Task.Run(() => FileProcessingServices.CsvLineToString(csvFile.Lines[i], csvFile.Delimiter, csvFile.BlockIdentifier));

                    progress.Report(i + 1);

                    stringBuilder.Append(processedLine);

                    if (i < csvFile.Lines.Count - 1)
                    {
                        stringBuilder.Append(Environment.NewLine);
                    }
                }

                return stringBuilder.ToString();
            }).Result;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
