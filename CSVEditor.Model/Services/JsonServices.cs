using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;

namespace CSVEditor.Model.Services
{
    public class JsonServices
    {
        public static bool SaveAppOptions(AppOptions options, string fullPath)
        {
            var jsoned = JsonSerializer.Serialize(options);
            //var jsoned = JsonConvert.SerializeObject(options);

            try
            {
                File.WriteAllText(fullPath, jsoned);
                Console.WriteLine($"Options saved to: {fullPath}");
            }
            catch (Exception e)
            {
                Console.WriteLine("Error saving options:" + e.Message);
                return false;
            }

            return true;
        }

        public static AppOptions LoadAppOptions(string fullPath)
        {
            AppOptions loadedOptions = null;

            try
            {
                var loadedOptionsJson = File.ReadAllText(fullPath);
                loadedOptions = JsonSerializer.Deserialize<AppOptions>(loadedOptionsJson);
                Console.WriteLine($"Options loaded from: {fullPath}");
            }
            catch (Exception e)
            {
                Console.WriteLine("Error loading options:" + e.Message);
            }

            return loadedOptions;
        }
    }
}
