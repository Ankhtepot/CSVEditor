using CSVEditor.Model;
using CSVEditor.ViewModel.Abstracts;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using static CSVEditor.Model.HelperClasses.Enums;

namespace CSVEditor.ViewModel.BackgroundWorkers
{

    //NONSENCE - WORK OUT TASK APPROACH IN CSVFILE!!!!!!!!!!!!!!!!!!!!!!!!!
    //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
    //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
    //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
    //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
    //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
    //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
    //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
    //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
    public class GetTextFromCsvFileWorker : AbstractEditorVMWorker
    {
        GetTextFromCsvFileWorker(EditorVM editorVM) : base(editorVM) { } 

        protected override void _DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = (BackgroundWorker)sender;
            worker.ReportProgress(0);
            VM.AsyncVM.WorkingStatus = WorkStatus.Working;

            var workingFile = CsvFile.Copy(VM.SelectedCsvFile);

            var sb = new StringBuilder();
            foreach(var header in workingFile.HeadersStrings)
            {
                sb.Append($"header{workingFile.Delimiter}");
            }

            sb.Append('\n');

            var linesCount = workingFile.Lines.Count;
            var progressStep = (100d / linesCount);
            var currentProgress = 0d;

            for (int line = 0; line < linesCount; line++)
            {
                foreach (var column in workingFile.Lines[line])
                {
                    if (column.Contains(workingFile.BlockIdentifier))
                    {
                        sb.Append($"{workingFile.BlockIdentifier}column{workingFile.BlockIdentifier}");
                    }
                    else
                    {
                        sb.Append(column);
                    }
                }

                if (line != linesCount - 1)
                {
                    sb.Append('\n');
                }

                currentProgress += progressStep;
                worker.ReportProgress((int)currentProgress);

                if (worker.CancellationPending == true)
                {
                    e.Cancel = true;
                }
            }
        }

        protected override void _ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            VM.AsyncVM.WorkProgress = e.ProgressPercentage;
        }
    }
}
