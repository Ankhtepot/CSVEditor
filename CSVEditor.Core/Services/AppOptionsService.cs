using System;
using System.IO;
using CSVEditor.Core.Extensions;
using CSVEditor.Core.HelperClasses;
using CSVEditor.Core.Properties;

namespace CSVEditor.Core.Services;

public static class AppOptionsService
{
    public static AppOptions AppOptions { get; private set; }
    public static bool? IsLoggedInToGit => AppOptions?.GitOptions?.IsAuthenticated;
    
    private const string DebugLogFilePathFragment = "Logs\\debug.log";
    private static string DebugLogFilePath => Path.Combine(FileSystemService.ConfigurationFolderPath, DebugLogFilePathFragment);
    private const string ProductionLogFilePathFragment = "Logs\\CSVEditorLog.log";
    private static string ProductionLogFilePath => Path.Combine(FileSystemService.ConfigurationFolderPath, ProductionLogFilePathFragment);

    public static AppOptions LoadAppOptions()
    {
        try
        {
            AppOptions = JsonServices.DeserializeJson<AppOptions>(
                Path.Combine(FileSystemService.ConfigurationFolderPath, Constants.APP_OPTIONS_FILE_NAME),
                Resources.AppOptionsName);
            
            SetLogRelatedFields();
        }
        catch (Exception e)
        {
            string errorMessage = $"{string.Format(Resources.AppOptionsLoadError, Constants.APP_OPTIONS_FILE_NAME)}" +
                                  $"{Environment.NewLine}" +
                                  $"{string.Format(Resources.ErrorWithPreface, e.Message)}";
            Console.WriteLine(errorMessage);
            throw new InvalidOperationException(errorMessage);
        }

        return AppOptions;
    }

    public static void SaveAppOptions()
    {
        try
        {
            JsonServices.SerializeJson(AppOptions,
                Path.Combine(FileSystemService.ConfigurationFolderPath, Constants.APP_OPTIONS_FILE_NAME),
                Resources.AppOptionsName);
        }
        catch(Exception e)
        {
            string errorMessage = $"{string.Format(Resources.AppOptionsSaveError, Constants.APP_OPTIONS_FILE_NAME)}" +
                                  $"{Environment.NewLine} " +
                                  $"{string.Format(Resources.ErrorWithPreface, e.Message)}";
            Console.WriteLine(errorMessage);
            throw new InvalidOperationException(errorMessage);
        }
    }

    public static void SetDefaultAppOptions()
    {
        AppOptions = new AppOptions();
        SaveAppOptions();
    }

    public static void SetGitOptions(GitOptions sourceGitOptions)
    {
        AppOptions.GitOptions.CommitMessage = sourceGitOptions.CommitMessage;
        AppOptions.GitOptions.UserName = sourceGitOptions.UserName;
        AppOptions.GitOptions.Email = sourceGitOptions.Email;
        AppOptions.GitOptions.Password = sourceGitOptions.Password;
        AppOptions.GitOptions.UseToken = sourceGitOptions.UseToken;
        AppOptions.GitOptions.RemoteRepositoryLink = sourceGitOptions.RemoteRepositoryLink;
        AppOptions.GitOptions.RemoteName = sourceGitOptions.RemoteName;
        AppOptions.GitOptions.IsAuthenticated = sourceGitOptions.IsAuthenticated;
    }
    
    private static void SetLogRelatedFields()
    {
#if DEBUG
        AppOptions.LogFilePath = DebugLogFilePath;
        AppOptions.LogLevel = LogLevel.Debug;
        AppOptions.LogIntoConsole = true;
#else
        AppOptions.LogFilePath = ProductionLogFilePath;
        AppOptions.LogLevel = LogLevel.Info;
        AppOptions.LogIntoConsole = false;
#endif
        try
        {
            Path.GetDirectoryName(AppOptions.LogFilePath).CreateDirectoryIfNotExists();
            Logger.SetOptions(
                AppOptions.LogFilePath,
                false,
                AppOptions.LogLevel,
                AppOptions.LogIntoConsole);
        }
        catch (Exception e)
        {
            Console.WriteLine($@"Error creating log directory: {e.Message}");
        }
    }
}