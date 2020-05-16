using CSVEditor.Model;
using CSVEditor.ViewModel.Abstracts;
using System.ComponentModel;
using static CSVEditor.Model.Enums;

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
            VM.SelectedCsvFile = new CsvFile((string)e.Argument, worker);

            if (worker.CancellationPending == true)
            {
                e.Cancel = true;
            }
        }

        protected override void _ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            VM.AsyncVM.WorkProgress += 100;
        }
    }
}

