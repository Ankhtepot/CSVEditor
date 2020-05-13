using CSVEditor.Model;
using CSVEditor.ViewModel.Abstracts;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using static CSVEditor.Model.Enums;

namespace CSVEditor.ViewModel.BackgroundWorkers
{
    public class LoadDirectoriesWithCsvWorker : EditorVMWorkerAbs
    {
        public LoadDirectoriesWithCsvWorker(EditorVM vM) : base (vM) {}

        protected override void _DoWork(object sender, DoWorkEventArgs e)
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

        protected override void _ProgressChanged(object sender, ProgressChangedEventArgs e)
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
    }
}
