using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Classes.Resources
{
    public class JSONParser
    {
        /// <summary>
        /// Generic method to parse JSON file.
        /// </summary>
        /// <typeparam name="T">Type to deserialize</typeparam>
        /// <param name="fileName">File Name</param>
        /// <returns></returns>
        public static List<T> ParseJson<T>(string fileName)
        {
            if (!File.Exists(fileName))
            {
                return null;
            }

            using (StreamReader r = new StreamReader(fileName))
            {
                try
                {
                    string json = r.ReadToEnd();
                    return JsonConvert.DeserializeObject<List<T>>(json);
                }
                catch (System.Exception e)
                {
                    return null;
                }

            }
        }
    }
}
