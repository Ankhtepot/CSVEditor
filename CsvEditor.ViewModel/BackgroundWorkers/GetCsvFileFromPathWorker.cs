﻿using CSVEditor.Model;
using CSVEditor.ViewModel.Abstracts;
using System.ComponentModel;
using static CSVEditor.Model.Enums;

namespace CSVEditor.ViewModel.BackgroundWorkers
{
    public class GetCsvFileFromPathWorker : EditorVMWorkerAbs
    {
        public GetCsvFileFromPathWorker(EditorVM vM) : base(vM) {}

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
