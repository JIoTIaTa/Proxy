using System;
using System.IO;
using System.Net;
using System.Xml;

namespace Proxy
{
    class WebFile
    {
        public WebFile()
        {        
        }
        public void Download(string fileUrl, string localPath)
        {
            Uri url = new Uri(fileUrl);

            using (var client = new WebClient())
            {
                client.DownloadFile(url, localPath);
            }
        }
    }
}