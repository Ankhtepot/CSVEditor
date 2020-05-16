using CSVEditor.Model;
using CSVEditor.ViewModel.Abstracts;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using static CSVEditor.Model.Enums;

namespace CSVEditor.ViewModel.BackgroundWorkers
{
    public class LoadDirectoriesWithCsvWorker : AbstractEditorVMWorker
    {
        public LoadDirectoriesWithCsvWorker(EditorVM vM) : base(vM) { }

        protected override void _DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = (BackgroundWorker)sender;

            VM.AsyncVM.WorkingStatus = WorkStatus.Working;

            worker.ReportProgress(VM.AsyncVM.WorkProgress + 100); // +100 to always trigger change for progress value >=100
            var path = e.Argument as string;

            var directories = new List<string> { path };
            var foundDirectories = FileSystemServices.GetDirectoriesFromRootPath(path, worker);

            if (worker.CancellationPending == true || foundDirectories == null)
            {
                e.Cancel = true;
                return;
            }

            directories = directories.Concat(foundDirectories).ToList();

            Console.WriteLine($"BW finished loading directories. Directories found: {directories.Count}");

            worker.ReportProgress(25);

            double progressStep = (100d / (double)directories.Count);
            double currentProgress = 0;
            directories.ForEach(directory =>
            {
                if (worker.CancellationPending == true)
                {
                    e.Cancel = true;
                    return;
                }
                else
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
                }
            });            
        }

    protected override void _ProgressChanged(object sender, ProgressChangedEventArgs e)
    {
        if (e.UserState == null)
        {
            VM.AsyncVM.WorkProgress = e.ProgressPercentage;
            return;
        }

        var foundDirectoryWithCsv = e.UserState as DirectoryWithCsv;
        VM.AsyncVM.WorkProgress = e.ProgressPercentage;
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
