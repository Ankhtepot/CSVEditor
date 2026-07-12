using System;

namespace CSVEditor.Core.Services;

public static class EventManager
{
    //******** Subscribers ********
    public static event Action OnGitOptionsWindowRequested;
    
    //******** Methods to trigger events ********
    public static void TriggerGitOptionsWindowRequested() => OnGitOptionsWindowRequested?.Invoke();
    
}