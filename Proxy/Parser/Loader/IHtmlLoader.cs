using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Proxy.Parser.Facebook
{
    interface IHtmlLoader
    {
        IWebProxy WebProxy { get; set; }

        /// <summary>
        /// Установить прокси сервер
        /// </summary>
        /// <param name="webProxy"> прокси сервер</param>
        void SetWebProxy(IWebProxy webProxy);
        Task<string> GetResponseCode(string url);
        Task<string> GetResponseCodeByProxy(string url);
        Task<string> GetResponseText(string url);
        Task<string> Connect(string currentUrl);
    }
}
