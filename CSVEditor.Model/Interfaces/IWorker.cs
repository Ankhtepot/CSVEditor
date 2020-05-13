using System;
using System.Collections.Generic;
using System.Text;

namespace CSVEditor.Model.Interfaces
{
    public interface IWorker
    {
        public void RunAsync(object argument);
        public void CancelAsync();
    }
}
