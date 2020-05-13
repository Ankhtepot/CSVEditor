using CSVEditor.Model;
using CSVEditor.Model.Interfaces;
using CSVEditor.ViewModel.Abstracts;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using static CSVEditor.Model.Enums;

namespace CSVEditor.ViewModel.BackgroundWorkers
{
    public class GetCsvFileFromPathWorker : WorkerAbs
    {
        public GetCsvFileFromPathWorker(EditorVM vM) : base(vM)
        {
        }

        protected override void _DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = (BackgroundWorker)sender;
            worker.ReportProgress(100);
            VM.WorkingStatus = WorkStatus.Working;
            VM.SelectedCsvFile = new CsvFile((string)e.Argument);
        }

        protected override void _ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            VM.WorkProgress = 100;
        }
    }
}

