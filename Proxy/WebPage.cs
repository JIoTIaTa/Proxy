using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace Proxy
{
    class WebPage
    {
        WebProxy webProxy;
        WebProxy WebProxy
        {
            get { return webProxy; }
            set { webProxy = value; }
        }
        
        public WebPage(WebProxy webProxy)
        {
            this.webProxy = webProxy;
        }
        public WebPage()
        {

        }
        public async void Connect(string webpage)
        { 
            try
            {
                WebRequest wrq = WebRequest.Create(webpage);
                wrq.Proxy = webProxy;
                WebResponse wrs = await wrq.GetResponseAsync();
            }
            catch (Exception exception)
            {
            }            
        }


    }
}