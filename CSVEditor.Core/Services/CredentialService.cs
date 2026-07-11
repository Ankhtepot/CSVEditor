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

            using var cred = new Credential();
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

            using var cred = new Credential();
            cred.Target = GetTarget(normalizedUserName);
            if (cred.Load())
            {
                return cred.Password;
            }

            string trimmedUserName = username.Trim();
            if (!string.Equals(trimmedUserName, normalizedUserName, StringComparison.Ordinal))
            {
                using var legacyPerUserCred = new Credential();
                legacyPerUserCred.Target = $"{CredentialName}_{trimmedUserName}";
                if (legacyPerUserCred.Load())
                {
                    string token = legacyPerUserCred.Password;
                    SaveToken(trimmedUserName, token);
                    return token;
                }
            }

            return null;
        }

        public static string GetLegacyToken()
        {
            using var cred = new Credential();
            cred.Target = CredentialName;
            return cred.Load() ? cred.Password : null;
        }

        public static void DeleteToken(string username)
        {
            string normalizedUserName = NormalizeUserName(username);
            if (!string.IsNullOrEmpty(normalizedUserName))
            {
                using var cred = new Credential();
                cred.Target = GetTarget(normalizedUserName);
                cred.Delete();
            }

            if (!string.IsNullOrWhiteSpace(username))
            {
                string trimmedUserName = username.Trim();
                if (!string.Equals(trimmedUserName, normalizedUserName, StringComparison.Ordinal))
                {
                    using var rawUserCred = new Credential();
                    rawUserCred.Target = $"{CredentialName}_{trimmedUserName}";
                    rawUserCred.Delete();
                }
            }

            using (var cred = new Credential())
            {
                cred.Target = CredentialName;
                cred.Delete();
            }
        }
    }
}
