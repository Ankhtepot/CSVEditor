using CSVEditor.Model;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using static CSVEditor.Model.Enums;

namespace CSVEditor.ViewModel.BackgroundWorkers
{
    public class GetCsvFileFromPathWorker
    {
        public BackgroundWorker Worker;

        private static GetCsvFileFromPathWorker instance;

        private EditorVM VM { get; set; }

        public GetCsvFileFromPathWorker(EditorVM vM)
        {
            VM = vM ?? throw new ArgumentNullException(nameof(vM));

            Worker = new BackgroundWorker();
            Worker.WorkerReportsProgress = true;
            Worker.WorkerSupportsCancellation = true;
            Worker.DoWork += _DoWork;
            Worker.ProgressChanged += _ReportProgress;
            Worker.RunWorkerCompleted += _Completed;
        }

        public static GetCsvFileFromPathWorker CreateWith(EditorVM VM)
        {
            if (instance != null)
            {                
                return instance;
            }

            instance = new GetCsvFileFromPathWorker(VM);
            return instance;
        }

        public void RunAsync(string forPath)
        {
            Worker.RunWorkerAsync(forPath);
        }

        private void _DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = (BackgroundWorker)sender;
            worker.ReportProgress(100);
            VM.WorkingStatus = WorkStatus.Working;
            VM.SelectedCsvFile = new CsvFile((string)e.Argument);
        }

        private void _ReportProgress(object sender, ProgressChangedEventArgs e)
        {
            VM.WorkProgress = 100;
        }

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

            Console.WriteLine($"BW:GetCsvFileFromPathWorker - Completed status: {resultInfo}");
        }
    }
}

