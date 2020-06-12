using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;

namespace CSVEditor.Model.Services
{
    public class JsonServices
    {
        public static bool SerializeJson<T>(T source, string fullPath, string referencedName = "")
        {
            var jsonedSource = JsonSerializer.Serialize(source);

            try
            {
                File.WriteAllText(fullPath, jsonedSource);
                Console.WriteLine($"{referencedName} saved to: {fullPath}");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error saving {referencedName}: {e.Message}");
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
                Console.WriteLine($"{referencedName} loaded from: {fullPath}");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error loading {referencedName}: {e.Message}");
            }

            return deserializedJson;
        }

        public static List<CsvFileConfiguration> LoadCsvFilesConfigurations(string fullPath)
        {
            return null;
        }
    }
}
