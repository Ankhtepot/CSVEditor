using System;
using System.Linq;
using System.Threading.Tasks;
using CSVEditor.Core.Extensions;
using CSVEditor.Core.HelperClasses;
using CSVEditor.Core.Properties;
using LibGit2Sharp;
using Octokit;
using Branch = LibGit2Sharp.Branch;
using Credentials = Octokit.Credentials;
using Repository = LibGit2Sharp.Repository;
using Signature = LibGit2Sharp.Signature;

namespace CSVEditor.Core.Services;

public static class GitService
{
    public static Repository CurrentRepository { get; private set; }

    public static void SetRepository(string path)
    {
        try
        {
            Console.WriteLine(Resources.SettingUpRepositoryText);
            CurrentRepository?.Dispose();
            CurrentRepository = new Repository(path);
            IsRepositoryUnstaged();
        }
        catch (RepositoryNotFoundException e)
        {
            Console.WriteLine(Resources.PathNotValidRepositoryFormat, path, e.Message);
        }
    }

    public static bool IsRepositoryUnstaged(Repository repo = null)
    {
        repo ??= CurrentRepository;
        return repo?.RetrieveStatus().IsDirty == true;
    }

    /// <summary>
    /// Gets the current git status of the repository as a string, concretely only changed files.
    /// </summary>
    public static string GetGitStatus()
    {
        RepositoryStatus status = CurrentRepository?.RetrieveStatus();
        return status == null
            ? string.Format(Resources.GitStatusFailed)
            : status.Modified.ToList().Count > 0
                ? CurrentRepository.RetrieveStatus().Modified
                    .Select(e => e.FilePath)
                    .ToHumanReadableString('\n')
                : Resources.GitStatusNoChanges;
    }

    private static bool StageRepository(Repository repo)
    {
        try
        {
            Commands.Stage(repo, "*");
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine(Resources.ErrorStagingRepositoryFormat, repo.Info.WorkingDirectory, e.Message);
            return false;
        }
    }

    public static bool CommitRepository(GitOptions gitOpts)
    {
        Signature authorSignature = new(gitOpts.UserName, gitOpts.Email, DateTimeOffset.Now);
        try
        {
            using Repository repo = new(CurrentRepository.Info.Path);
            if (IsRepositoryUnstaged(repo))
            {
                StageRepository(repo);
                Console.WriteLine(Resources.GitRepositoryStagedMessage);
            }
            repo.Commit(gitOpts.CommitMessage, authorSignature, authorSignature);
            Console.WriteLine(Resources.GitRepositoryCommittedMessage);
            SetRepository(repo.Info.WorkingDirectory);
            return true;
        }
        catch (EmptyCommitException)
        {
            Console.WriteLine(Resources.NotCommitedError);
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine(Resources.UnexpectedErrorDuringCommitFormat, e.Message);
            return false;
        }
    }

    public static void Pull(GitOptions gitOpts)
    {
        using Repository repo = new(CurrentRepository.Info.Path);
        string password = gitOpts.UseToken
            ? CredentialService.GetToken(gitOpts.UserName) ?? gitOpts.Password
            : gitOpts.Password;
        PullOptions options = new()
        {
            FetchOptions = new FetchOptions
            {
                CredentialsProvider = (_, _, _) =>
                    new UsernamePasswordCredentials
                    {
                        Username = gitOpts.UserName,
                        Password = password
                    }
            }
        };
        Signature signature = new(gitOpts.UserName, gitOpts.Email, DateTimeOffset.Now);
        Commands.Pull(repo, signature, options);
        SetRepository(repo.Info.WorkingDirectory);
    }

    public static void Push(GitOptions gitOpts)
    {
        using Repository repo = new(CurrentRepository.Info.Path);
        string password = gitOpts.UseToken
            ? CredentialService.GetToken(gitOpts.UserName) ?? gitOpts.Password
            : gitOpts.Password;

        Remote remote = repo.Network.Remotes[gitOpts.RemoteName];
        if (remote == null)
        {
            if (!string.IsNullOrEmpty(gitOpts.RemoteRepositoryLink) &&
                !gitOpts.RemoteRepositoryLink.Contains("<"))
            {
                Console.WriteLine(Resources.RemoteOriginNotFoundAddingFormat, gitOpts.RemoteName, gitOpts.RemoteRepositoryLink);
                remote = repo.Network.Remotes.Add(gitOpts.RemoteName, gitOpts.RemoteRepositoryLink);
            }
            else
            {
                Console.WriteLine(Resources.RemoteOriginNotFoundNoLinkText, gitOpts.RemoteName);
                return;
            }
        }
        else if (!string.IsNullOrEmpty(gitOpts.RemoteRepositoryLink) &&
                 !gitOpts.RemoteRepositoryLink.Contains('<') &&
                 remote.Url != gitOpts.RemoteRepositoryLink)
        {
            Console.WriteLine(Resources.GitUpdatingRemoteNameMessage, remote.Name, gitOpts.RemoteRepositoryLink);
            repo.Network.Remotes.Update(remote.Name, r => r.Url = gitOpts.RemoteRepositoryLink);
            remote = repo.Network.Remotes[gitOpts.RemoteName];
        }

        PushOptions options = new()
        {
            CredentialsProvider = (_, _, _) =>
            {
                Console.WriteLine(Resources.GitProvidingCredentialsMessage, gitOpts.UserName);
                return new UsernamePasswordCredentials
                {
                    Username = gitOpts.UserName,
                    Password = password
                };
            },
            OnPushStatusError = error =>
            {
                Console.WriteLine(Resources.GitPushStatusErrorMessage, error.Reference, error.Message);
            }
        };

        if (repo.Info.IsHeadDetached)
        {
            throw new InvalidOperationException(Resources.GitDetachedHeadErrorMessage);
        }

        string pushRefSpec = $"{repo.Head.CanonicalName}:{repo.Head.CanonicalName}";
        string sha = repo.Head.Tip?.Sha ?? "no commits";
        BranchTrackingDetails tracking = repo.Head.TrackingDetails;
        string aheadStr = tracking != null ? (tracking.AheadBy?.ToString() ?? "0") : "unknown (no tracking)";

        CommitRepository(gitOpts);

        Console.WriteLine(Resources.GitPushingLocal, repo.Head.FriendlyName, sha, remote.Name, remote.Url);
        Console.WriteLine(Resources.GitPushBranchStateInfoMessage, pushRefSpec, aheadStr);
        repo.Network.Push(remote, [pushRefSpec], options);

        Branch localBranch = repo.Head;
        if (localBranch.TrackedBranch == null)
        {
            repo.Branches.Update(localBranch,
                b => b.Remote = remote.Name,
                b => b.UpstreamBranch = localBranch.CanonicalName);
            Console.WriteLine(Resources.GitSettingUpstreamBranch, localBranch.FriendlyName, remote.Name, localBranch.FriendlyName);
        }

        Console.WriteLine(Resources.GitRepositoryPushedMessage, remote.Name);
        SetRepository(repo.Info.WorkingDirectory);
    }

    /// <summary>
    /// Verifies GitHub login by calling the GitHub API.
    /// Returns true if verified, false if the token is invalid/expired, null on network error.
    /// </summary>
    public static async Task<bool?> VerifyGitHubLoginAsync()
    {
        GitOptions gitOpts = AppOptionsService.AppOptions?.GitOptions;
        if (gitOpts is not {IsAuthenticated: true}) return null;

        string token = gitOpts.UseToken
            ? CredentialService.GetToken(gitOpts.UserName) ?? gitOpts.Password
            : gitOpts.Password;
        if (string.IsNullOrEmpty(token)) return false;

        try
        {
            GitHubClient client = new(new ProductHeaderValue("CSVEditor"))
            {
                Credentials = new Credentials(token)
            };
            await client.User.Current();
            AppOptionsService.AppOptions.GitOptions.Password = token;
            return true;
        }
        catch (AuthorizationException)
        {
            return false;
        }
        catch (Exception)
        {
            return null;
        }
    }
}