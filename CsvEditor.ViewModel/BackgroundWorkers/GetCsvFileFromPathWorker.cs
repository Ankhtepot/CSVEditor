using CSVEditor.Core;
using CSVEditor.ViewModel.Abstracts;
using System.ComponentModel;
using static CSVEditor.Core.HelperClasses.Enums;

namespace CSVEditor.ViewModel.BackgroundWorkers
{
    public class GetCsvFileFromPathWorker : AbstractEditorVMWorker
    {
        public GetCsvFileFromPathWorker(EditorVM vM) : base(vM) {}

        protected override void _DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = (BackgroundWorker)sender;
            worker.ReportProgress(VM.AsyncVM.WorkProgress + 100);
            VM.AsyncVM.WorkingStatus = WorkStatus.Working;

            CsvFile loadedCsvFile = new((string)e.Argument, worker);
            e.Result = loadedCsvFile;

            if (worker.CancellationPending)
            {
                e.Cancel = true;
            }
        }        

        protected override void _ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            VM.AsyncVM.WorkProgress += 100;
        }

        protected override void _Completed(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error == null && !e.Cancelled && e.Result is CsvFile loadedCsvFile)
            {
                VM.SelectedCsvFile = loadedCsvFile;
            }
            base._Completed(sender, e);
        }
    }
}

