using CredentialManagement;

namespace CSVEditor.Core.Services
{
    public static class CredentialService
    {
        private const string CredentialName = "CSVEditor_GitHub_Token";

        public static void SaveToken(string username, string token)
        {
            if (string.IsNullOrEmpty(username)) return;
            using var cred = new Credential();
            cred.Target = $"{CredentialName}_{username}";
            cred.Password = token;
            cred.Type = CredentialType.Generic;
            cred.PersistanceType = PersistanceType.LocalComputer;
            cred.Save();
        }

        public static string GetToken(string username)
        {
            if (!string.IsNullOrEmpty(username))
            {
                using var cred = new Credential();
                cred.Target = $"{CredentialName}_{username}";
                if (cred.Load())
                {
                    return cred.Password;
                }
            }

            // Fallback to legacy single-token storage
            using (var cred = new Credential())
            {
                cred.Target = CredentialName;
                if (cred.Load())
                {
                    return cred.Password;
                }
            }
            return null;
        }

        public static void DeleteToken(string username)
        {
            if (!string.IsNullOrEmpty(username))
            {
                using var cred = new Credential();
                cred.Target = $"{CredentialName}_{username}";
                cred.Delete();
            }

            using (var cred = new Credential())
            {
                cred.Target = CredentialName;
                cred.Delete();
            }
        }
    }
}
