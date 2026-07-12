using CredentialManagement;
using System;

namespace CSVEditor.Core.Services
{
    public static class CredentialService
    {
        private const string CredentialName = "CSVEditor_GitHub_Token";
        
        private static string NormalizeUserName(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                return null;
            }

            return username.Trim().ToLowerInvariant();
        }

        private static string GetTarget(string normalizedUserName) => $"{CredentialName}_{normalizedUserName}";

        public static void SaveToken(string username, string token)
        {
            string normalizedUserName = NormalizeUserName(username);
            if (string.IsNullOrEmpty(normalizedUserName) || string.IsNullOrEmpty(token)) return;

            using Credential cred = new();
            cred.Target = GetTarget(normalizedUserName);
            cred.Password = token;
            cred.Type = CredentialType.Generic;
            cred.PersistanceType = PersistanceType.LocalComputer;
            cred.Save();
        }

        public static string GetToken(string username)
        {
            string normalizedUserName = NormalizeUserName(username);
            if (string.IsNullOrEmpty(normalizedUserName)) return null;

            using Credential cred = new();
            cred.Target = GetTarget(normalizedUserName);
            return cred.Load() 
                ? cred.Password 
                : null;
        }

        public static void DeleteToken(string username)
        {
            string normalizedUserName = NormalizeUserName(username);
            if (!string.IsNullOrEmpty(normalizedUserName))
            {
                using Credential cred = new();
                cred.Target = GetTarget(normalizedUserName);
                cred.Delete();
            }

            if (!string.IsNullOrWhiteSpace(username))
            {
                string trimmedUserName = username.Trim();
                if (!string.Equals(trimmedUserName, normalizedUserName, StringComparison.Ordinal))
                {
                    using Credential rawUserCred = new();
                    rawUserCred.Target = $"{CredentialName}_{trimmedUserName}";
                    rawUserCred.Delete();
                }
            }

            using (Credential cred = new())
            {
                cred.Target = CredentialName;
                cred.Delete();
            }
        }
    }
}
