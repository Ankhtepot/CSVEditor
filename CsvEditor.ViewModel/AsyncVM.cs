using CSVEditor.Core;
using CSVEditor.Core.HelperClasses;
using CSVEditor.ViewModel.Abstracts;
using CSVEditor.ViewModel.BackgroundWorkers;
using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CSVEditor.Core.Services;
using static CSVEditor.Core.HelperClasses.Enums;

namespace CSVEditor.ViewModel
{
    public class AsyncVM : INotifyPropertyChanged
    {
        private const int UPDATE_PROGRESS_DELAY = 100;

        public string SelectedFileRaw
        {
            get;
            set
            {
                field = value;
                OnPropertyChanged();
            }
        }

        public bool IsConverterProcessing
        {
            get;
            set
            {
                field = value;
                OnPropertyChanged();
            }
        }

        public WorkStatus WorkingStatus
        {
            get;
            set
            {
                field = value;
                OnPropertyChanged();
            }
        }

        public int WorkProgress
        {
            get;
            set
            {
                field = value;
                OnPropertyChanged();
            }
        }

        public AbstractEditorVMWorker ActiveWorker //this field is being handled by AbstractEditorVMWorker
        {
            get;
            set
            {
                field = value;
                OnPropertyChanged();
            }
        }


        public EditorVM EditorVM
        {
            get;
            set
            {
                field = value;
                OnPropertyChanged();
            }
        }


        public AsyncVM(EditorVM editorVM)
        {
            EditorVM = editorVM;
            IsConverterProcessing = false;
            SelectedFileRaw = Constants.NO_FILE_SELECTED;
        }

        public async void SetRawTextFromAbsPath(string path)
        {
            CancellationTokenSource cts = new();

            UpdateProcessingFileTextTask(cts.Token);
            string rawText = await Task.Run(() => GetRawTextTask(cts, path), cts.Token);
            await Task.Delay(UPDATE_PROGRESS_DELAY * 3);
            SelectedFileRaw = rawText;
        }

        private string GetRawTextTask(CancellationTokenSource cts, string path)
        {
            Task<string> result = Task.Run( async () =>
            {
                string rawText = await Task.Run(() => FileProcessingServices.GetRawFileText(path));
                await cts.CancelAsync();
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
                    await Task.Delay(UPDATE_PROGRESS_DELAY, token);
                    int dotsCount = SelectedFileRaw.Count(ch => ch == '.');
                    SelectedFileRaw = dotsCount < 10 ? SelectedFileRaw + "." : SelectedFileRaw.Replace(".", ""); 
                }
            }, token);
        }

        public void LoadRepository()
        {
            EditorVM.SelectedFile = null;
            EditorVM.SelectedCsvFile = null;

            EditorVM.RootRepositoryPath = FileSystemService.QueryUserForRootRepositoryPath(Constants.SELECT_PROJECT_ROOT_DIRECTORY);

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
            if (csvFile == null)
            {
                return null;
            }

            WorkingStatus = WorkStatus.Working;

            if (csvFile.Lines.Count < 1)
            {
                WorkingStatus = WorkStatus.Done;
            }

            double progressStep = 100d / csvFile.Lines.Count;
            WorkProgress = 0;

            Progress<int> progress = new(lineNr =>
            {
                WorkProgress = (int)((lineNr + 1) * progressStep); // +1 for headers line
            });

            string csvText = await Task.Run(() => ConvertCsvFileToTextTask(csvFile, progress));

            WorkingStatus = WorkStatus.Done;

            return csvText;
        }

        private string ConvertCsvFileToTextTask(CsvFile csvFile, IProgress<int> progress)
        {
            return Task.Run(async () =>
            {
                StringBuilder stringBuilder = new(FileProcessingServices.HeadersLineToStringLine(csvFile));

                for (int i = 0; i < csvFile.Lines.Count; i++)
                {
                    string processedLine = await Task.Run(() => FileProcessingServices.CsvLineToString(csvFile.Lines[i], csvFile.Delimiter, csvFile.BlockIdentifier));

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
