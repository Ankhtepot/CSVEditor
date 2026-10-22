using CSVEditor.Core.HelperClasses;
using CSVEditor.ViewModel.Abstracts;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using CSVEditor.Core.Services;
using static CSVEditor.Core.HelperClasses.Enums;

namespace CSVEditor.ViewModel.BackgroundWorkers
{
    public class LoadDirectoriesWithCsvWorker(EditorVM vM) : AbstractEditorVMWorker(vM)
    {
        protected override void _DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = (BackgroundWorker)sender;

            VM.AsyncVM.WorkingStatus = WorkStatus.Working;

            worker.ReportProgress(VM.AsyncVM.WorkProgress + 100); // +100 to always trigger change for progress value >=100
            string path = e.Argument as string;

            List<string> directories = [path];
            List<string> foundDirectories = FileSystemService.GetDirectoriesFromRootPath(path, worker);

            if (worker.CancellationPending || foundDirectories == null)
            {
                e.Cancel = true;
                return;
            }

            directories = directories.Concat(foundDirectories).ToList();

            Console.WriteLine($@"BW finished loading directories. Directories found: {directories.Count}");

            worker.ReportProgress(25);

            double progressStep = (100d / directories.Count);
            double currentProgress = 0;
            directories.ForEach(directory =>
            {
                if (worker.CancellationPending)
                {
                    e.Cancel = true;
                }
                else
                {
                    currentProgress += progressStep;
                    DirectoryWithCsv foundDirectoryWithCsv = FileSystemService.ScanDirectory(path, directory);
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

            DirectoryWithCsv foundDirectoryWithCsv = e.UserState as DirectoryWithCsv;
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

        protected override void _Completed(object sender, RunWorkerCompletedEventArgs e)
        {
            base._Completed(sender, e);
            AppOptionsService.AppOptions.LastCsvFilesStructure = VM.CsvFilesStructure.ToList();
        }
    }
}
