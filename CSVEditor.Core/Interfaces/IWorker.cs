namespace CSVEditor.Core.Interfaces
{
    public interface IWorker
    {
        public void RunAsync(object argument);
        public void CancelAsync();
    }
}
