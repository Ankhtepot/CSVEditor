using CSVEditor.Model;
using CSVEditor.ViewModel.Abstracts;
using System.ComponentModel;
using static CSVEditor.Model.HelperClasses.Enums;

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

            CsvFile loadedCsvFile = new CsvFile((string)e.Argument, worker);
            VM.SelectedCsvFile = loadedCsvFile;
            EditorVM.AppOptions.LastLoadedUneditedCsvFile = loadedCsvFile;

            if (worker.CancellationPending == true)
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
            base._Completed(sender, e);
        }
    }
}

