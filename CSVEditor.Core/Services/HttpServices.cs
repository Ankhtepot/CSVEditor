using System;
using System.Net.Http;

namespace CSVEditor.Core.Services
{
    public class HttpServices
    {
        private static readonly HttpClient _httpClient = new HttpClient();

        public static bool checkWebsite(string URL)
        {
            try
            {
                using var response = _httpClient.GetAsync(URL).GetAwaiter().GetResult();
                return response.IsSuccessStatusCode;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
