using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using CSVEditor.Core.HelperClasses;
using CSVEditor.Core.Properties;

namespace CSVEditor.Core.Services
{
    public class JsonServices
    {
        public static bool SerializeJson<T>(T source, string fullPath, string referencedName = "")
        {
            JsonSerializerOptions options = new();
            options.WriteIndented = true;

            string jsonedSource = JsonSerializer.Serialize(source, options);

            try
            {
                File.WriteAllText(fullPath, jsonedSource);
                Logger.LogInfo(string.Format(Resources.SavedToFormat, referencedName, fullPath));
            }
            catch (Exception e)
            {
                Logger.LogError(string.Format(Resources.ErrorSavingFormat, referencedName, e.Message));
                return false;
            }

            return true;
        }

        public static T DeserializeJson<T>(string fullPath, string referencedName = "")
        {
            T deserializedJson = default(T);

            try
            {
                string loadedJson = File.ReadAllText(fullPath);
                deserializedJson = JsonSerializer.Deserialize<T>(loadedJson);
                Logger.LogInfo(string.Format(Resources.LoadedFromFormat, referencedName, fullPath));
            }
            catch (Exception e)
            {
                Logger.LogError(string.Format(Resources.ErrorLoadingFormat, referencedName, e.Message));
                throw new InvalidOperationException();
            }

            return deserializedJson;
        }
    }
}
