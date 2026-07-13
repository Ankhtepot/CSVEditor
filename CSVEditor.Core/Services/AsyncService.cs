using System;
using System.Threading.Tasks;
using CSVEditor.Core.HelperClasses;

namespace CSVEditor.Core.Services;

public static class AsyncService
{
    public static async Task RunAsync(
        Func<Task> action,
        Action onCompleted = null,
        string errorPreface = null,
        Action<Exception> onError = null,
        bool showErrorMessageBox = false
        )
    {
        try
        {
            await action();
            onCompleted?.Invoke();
        }
        catch (Exception ex)
        {
            string errorMessage = $"{(string.IsNullOrEmpty(errorPreface) ? errorPreface : "")}: {ex.Message}";
            onError?.Invoke(ex);
            if (showErrorMessageBox)
            {
                MessageBoxHelper.ShowProcessErrorBox("Error", errorMessage);
            }
        }
    }
}