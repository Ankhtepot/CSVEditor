using System;
using System.IO;
using System.Runtime.CompilerServices;
using static System.IO.Path;

namespace CSVEditor.Core.HelperClasses;

public enum LogLevel
{
    Debug = 0,
    Info = 1,
    Warning = 2,
    Error = 3
}

public static class Logger
{
    public static string LogFilePath { get; private set; } = "log.txt";
    public static bool ShowWholeClassPath { get; private set; }
    public static LogLevel CurrentLogLevel { get; private set; } = LogLevel.Debug;
    public static bool LogIntoConsole { get; private set; } = true;
    
    private const int MaxFailCount = 10;
    private static int _failCount;
    
    public static void SetOptions(
        string logFilePath = "log.txt",
        bool showWholeClassPath = false,
        LogLevel logLevel = LogLevel.Debug,
        bool logIntoConsole = true)
    {
        LogFilePath = logFilePath;
        ShowWholeClassPath = showWholeClassPath;
        CurrentLogLevel = logLevel;
        LogIntoConsole = logIntoConsole;
    }
    
    public static void LogDebug(string message = "", [CallerFilePath] string classPath = "", [CallerMemberName] string memberName = "")
    {
        if (CurrentLogLevel > LogLevel.Debug) return;
        
        string logMessage = ResolveLoggedMessage(classPath,memberName,$"DEBUG: {message}");
        PropagateLogMessage(logMessage);
    }
    
    public static void LogInfo(string message = "", [CallerFilePath] string classPath = "", [CallerMemberName] string memberName = "")
    {
        if (CurrentLogLevel > LogLevel.Info) return;
        
        string logMessage = ResolveLoggedMessage(classPath,memberName,$"INFO: {message}");
        PropagateLogMessage(logMessage);
    }
    
    public static void LogWarning(string message = "", [CallerFilePath] string classPath = "", [CallerMemberName] string memberName = "")
    {
        if (CurrentLogLevel > LogLevel.Warning) return;
        
        string logMessage = ResolveLoggedMessage(classPath,memberName,$"WARNING: {message}");
        PropagateLogMessage(logMessage);
    }
    
    public static void LogError(string message = "", [CallerFilePath] string classPath = "", [CallerMemberName] string memberName = "")
    {
        if (CurrentLogLevel > LogLevel.Error) return;
        
        string logMessage = ResolveLoggedMessage(classPath,memberName,$"ERROR: {message}");
        PropagateLogMessage(logMessage);
    }
    
    private static string ResolveLoggedMessage(string classPath, string memberName, string message)
    {
        string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        string prefix = ShowWholeClassPath ? $"[{classPath}]: {memberName} -> " : $"{GetFileNameWithoutExtension(classPath)}: {memberName}";
        return $"{timestamp}: {prefix}:{message}";
    }
    
    private static void PropagateLogMessage(string logMessage)
    {
        if (LogIntoConsole)
        {
            Console.WriteLine(logMessage);
        }
        
        if(_failCount >= MaxFailCount || LogFilePath == null)
        {
            _failCount += 1;
            
            Console.WriteLine(@"Log file path is not set. Log message will not be written to a file.");
            return;
        }
        
        try
        {
            File.AppendAllText(LogFilePath, logMessage + Environment.NewLine);
        }
        catch (Exception ex)
        {
            _failCount += 1;
            Console.WriteLine($@"Failed to write to log file: {ex.Message}");
        }
    }
}