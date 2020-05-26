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

            try
            {
                File.WriteAllText(fullPath + "\\options.json", jsoned);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error saving options:" + e.Message);
                return false;
            }

            return true;
        }

        //public static bool LoadAppOptions
    }
}
