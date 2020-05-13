using CSVEditor.Model;
using CSVEditor.Model.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using static CSVEditor.Model.Enums;

namespace CSVEditor.ViewModel.Abstracts
{
    public abstract class WorkerAbs : IWorker
    {
        public BackgroundWorker Worker;

        protected EditorVM VM { get; set; }

        public WorkerAbs(EditorVM vM)
        {
            VM = vM ?? throw new ArgumentNullException(nameof(vM));

            Worker = new BackgroundWorker();
            Worker.DoWork += _DoWork;
            Worker.WorkerReportsProgress = true;
            Worker.WorkerSupportsCancellation = true;
            Worker.ProgressChanged += _ProgressChanged;
            Worker.RunWorkerCompleted += _Completed;
        }

        public void RunAsync(object forPath)
        {
            Worker.RunWorkerAsync((string)forPath);
        }

        public void CancelAsync()
        {
            Worker.CancelAsync();
        }

        protected abstract void _DoWork(object sender, DoWorkEventArgs e);

        protected abstract void _ProgressChanged(object sender, ProgressChangedEventArgs e);        

        private void _Completed(object sender, RunWorkerCompletedEventArgs e)
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

            Console.WriteLine($"BW:{GetType().Name} - Completed status: {resultInfo}");
        }
    }
}
