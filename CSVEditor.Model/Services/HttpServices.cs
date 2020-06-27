using System;
using System.Net;

namespace CSVEditor.Model.Services
{
    public class HttpServices
    {
        public static bool checkWebsite(string URL)
        {
            try
            {
                WebClient wc = new WebClient();
                string HTMLSource = wc.DownloadString(URL);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
