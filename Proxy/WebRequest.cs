using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Proxy.Parser;

namespace Proxy
{
    class WebRequest
    {
        WebProxy webProxy;
        private ParserWorker<string[]> parser;

        public ParserWorker<string[]> Parser
        {
            get { return parser; }
            set { parser = value; }
        }

        public event Action<object, string> NewLog;

        public WebProxy WebProxy
        {
            get { return webProxy; }
            set { webProxy = value; }
        }
        
        public WebRequest(WebProxy webProxy)
        {
            this.webProxy = webProxy;
        }
        public WebRequest()
        {

        }
        public async void Connect(string webpage)
        {             
            try
            {
                System.Net.WebRequest wrq = System.Net.WebRequest.Create(webpage);
                wrq.Proxy = webProxy;
                
                
                WebResponse wrs = await wrq.GetResponseAsync();
                wrq.Abort();
                wrs.Close();
                string result = $"{webpage} -> успешно";
                NewLog?.Invoke(this, result);
            }
            catch (Exception exception)
            {
                string result = $"{webpage} -> {exception.Message}";
                NewLog?.Invoke(this, result);
            }
        }

        public  void Parse(string webpage)
        {
            try
            {
                parser.WebProxy = this.webProxy;
                parser.Start(webpage);
            }
            catch (Exception exception)
            {
               
            }
        }

    }
}