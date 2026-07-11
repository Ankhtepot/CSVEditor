using System;

namespace CSVEditor.Core.Services;

public static class EventManager
{
    //******** Subscribers ********
    public static event Action OnGitOptionsWindowRequested;
    public static event Action OnGitOptionsWindowClosed;
    
    //******** Methods to trigger events ********
    public static void TriggerGitOptionsWindowRequested() => OnGitOptionsWindowRequested?.Invoke();
    public static void TriggerGitOptionsWindowClosed() => OnGitOptionsWindowClosed?.Invoke();
    
}