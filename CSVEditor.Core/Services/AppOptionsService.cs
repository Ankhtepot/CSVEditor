using System;
using System.IO;
using CSVEditor.Core.HelperClasses;
using CSVEditor.Core.Properties;

namespace CSVEditor.Core.Services;

public static class AppOptionsService
{
    public static AppOptions AppOptions { get; private set; }
    public static bool? IsLoggedInToGit => AppOptions?.GitOptions?.IsAuthenticated;

    public static AppOptions LoadAppOptions()
    {
        try
        {
            AppOptions = JsonServices.DeserializeJson<AppOptions>(
                Path.Combine(FileSystemService.ConfigurationFolderPath, Constants.APP_OPTIONS_FILE_NAME),
                Resources.AppOptionsName);
        }
        catch (Exception e)
        {
            string errorMessage =
                string.Format(Resources.AppOptionsLoadError, Constants.APP_OPTIONS_FILE_NAME, e.Message);
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
            string errorMessage =
                string.Format(Resources.AppOptionsSaveError, Constants.APP_OPTIONS_FILE_NAME, e.Message);
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
}