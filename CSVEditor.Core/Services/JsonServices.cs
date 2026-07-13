using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using CSVEditor.Core.Properties;

namespace CSVEditor.Core.Services
{
    public class JsonServices
    {
        public static bool SerializeJson<T>(T source, string fullPath, string referencedName = "")
        {
            var options = new JsonSerializerOptions() { WriteIndented = true };

            var jsonedSource = JsonSerializer.Serialize(source, options);

            try
            {
                File.WriteAllText(fullPath, jsonedSource);
                Console.WriteLine(Resources.SavedToFormat, referencedName, fullPath);
            }
            catch (Exception e)
            {
                Console.WriteLine(Resources.ErrorSavingFormat, referencedName, e.Message);
                return false;
            }

            return true;
        }

        public static T DeserializeJson<T>(string fullPath, string referencedName = "")
        {
            T deserializedJson = default(T);

            try
            {
                var loadedJson = File.ReadAllText(fullPath);
                deserializedJson = JsonSerializer.Deserialize<T>(loadedJson);
                Console.WriteLine(Resources.LoadedFromFormat, referencedName, fullPath);
            }
            catch (Exception e)
            {
                Console.WriteLine(Resources.ErrorLoadingFormat, referencedName, e.Message);
                throw new InvalidOperationException();
            }

            return deserializedJson;
        }
    }
}
