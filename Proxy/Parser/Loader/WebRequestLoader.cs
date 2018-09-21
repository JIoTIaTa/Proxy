using System;
using System.Net;
using System.Threading.Tasks;
using Proxy.Parser;
using Proxy.Parser.Facebook;

namespace Proxy
{
    class WebRequestLoader : IHtmlLoader
    {
        IWebProxy webProxy;

        public event Action<object, string> NewLog;

        public IWebProxy WebProxy
        {
            get { return webProxy; }
            set { webProxy = value; }
        }

        public void SetWebProxy(IWebProxy webProxy)
        {
            this.webProxy = webProxy;
        }

        public Task<string> GetResponseCode(string url)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetResponseCodeByProxy(string url)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetResponseText(string url)
        {
            throw new NotImplementedException();
        }

        public async Task<string> Connect(string currentUrl)
        {
            try
            {
                System.Net.WebRequest wrq = System.Net.WebRequest.Create(currentUrl);
                wrq.Proxy = webProxy;


                WebResponse wrs = await wrq.GetResponseAsync();
                wrq.Abort();
                wrs.Close();
                string result = $"{currentUrl} -> успешно";
                NewLog?.Invoke(this, result);
                return result;
            }
            catch (Exception exception)
            {
                string result = $"{currentUrl} -> {exception.Message}";
                NewLog?.Invoke(this, result);
                return result;
            }
        }

    }
}