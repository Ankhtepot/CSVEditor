using CSVEditor.Model;
using CSVEditor.Model.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using static CSVEditor.Model.Enums;

namespace CSVEditor.ViewModel.BackgroundWorkers
{
    public class LoadDirectoriesWithCsvWorker : IWorker
    {
        public BackgroundWorker Worker;

        private static LoadDirectoriesWithCsvWorker instance;

        private EditorVM VM { get; set; }

        public LoadDirectoriesWithCsvWorker(EditorVM vM)
        {
            VM = vM ?? throw new ArgumentNullException(nameof(vM));

            Worker = new BackgroundWorker();
            Worker.DoWork += loadDirectoryStructure_DoWork;
            Worker.WorkerReportsProgress = true;
            Worker.WorkerSupportsCancellation = true;
            Worker.ProgressChanged += loadDirectoryStructure_ProgressChanged;
            Worker.RunWorkerCompleted += loadDirectoryStructure_Completed;
        }

        public static LoadDirectoriesWithCsvWorker CreateWith(EditorVM VM)
        {
            if (instance != null)
            {
                return instance;
            }

            instance = new LoadDirectoriesWithCsvWorker(VM);
            return instance;
        }

        public void RunAsync(object forPath)
        {
            Worker.RunWorkerAsync((string)forPath);
        }

        public void CancelAsync()
        {
            Worker.CancelAsync();
        }

        private void loadDirectoryStructure_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = (BackgroundWorker)sender;
            worker.ReportProgress(100);
            VM.WorkingStatus = WorkStatus.Working;
            var path = e.Argument as string;

            var directories = new List<string> { path };
            var foundDirectories = FileSystemServices.GetDirectoriesFromRootPath(path);

            if (foundDirectories == null)
            {
                worker.CancelAsync();
                return;
            }

            directories = directories.Concat(foundDirectories).ToList();

            Console.WriteLine($"BW finished loading directories. Directories found: {directories.Count}");

            worker.ReportProgress(25);

            double progressStep = (100d / (double)directories.Count);
            double currentProgress = 0;
            directories.ForEach(directory =>
            {
                currentProgress += progressStep;
                var foundDirectoryWithCsv = FileSystemServices.ScanDirectory(path, directory);
                if (foundDirectoryWithCsv != null)
                {
                    worker.ReportProgress((int)currentProgress, foundDirectoryWithCsv);
                }
                else
                {
                    worker.ReportProgress((int)currentProgress);
                }

            });            
        }

        private void loadDirectoryStructure_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e.UserState == null)
            {
                VM.WorkProgress = e.ProgressPercentage;
                return;
            }

            var foundDirectoryWithCsv = e.UserState as DirectoryWithCsv;
            VM.WorkProgress = e.ProgressPercentage;
            if (foundDirectoryWithCsv != null)
            {
                VM.CsvFilesStructure.Add(foundDirectoryWithCsv);
            }

            if (VM.CsvFilesStructure.Count == 0)
            {
                VM.CsvFilesStructure.Add(VM.DEFAULT_DIRECTORY);
            }
        }

        private void loadDirectoryStructure_Completed(object sender, RunWorkerCompletedEventArgs e)
        {
            var resultInfo = "";
            if (e.Cancelled == true)
            {
                resultInfo = "Canceled!";
                VM.WorkingStatus = WorkStatus.Canceled;
            }
            else if (e.Error != null)
            {
                resultInfo = "Error: " + e.Error.Message;
                VM.WorkingStatus = WorkStatus.Error;
            }
            else
            {
                resultInfo = "Done!";
                VM.WorkingStatus = WorkStatus.Done;
            }

            Console.WriteLine($"BW:LoadDirectoriesFromPathWorker - Completed status: {resultInfo}");
        }
    }
}
