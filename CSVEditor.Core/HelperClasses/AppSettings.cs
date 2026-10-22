namespace CSVEditor.Core.HelperClasses;

/// <summary>
/// Class representing application global settings, not persisted to disk, but used for runtime configuration and state management.
/// </summary>
public static class AppSettings
{
    public static string LogFilePath { get; set; }
    public static LogLevel LogLevel { get; set; } = LogLevel.Info;
    public static bool LogIntoConsole { get; set; } = true;
}