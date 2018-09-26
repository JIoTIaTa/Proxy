using System;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;
using AngleSharp.Parser.Html;
using Proxy.Parser.Facebook;

namespace Proxy.Parser
{
    class ParserWorker<T> where T : class
    {
        IParser<T> parser;

        IHtmlLoader loader;

        IWebProxy webProxy;

        bool isActive;


        #region Properties

        public IParser<T> Parser
        {
            get
            {
                return parser;
            }
            set
            {
                parser = value;
            }
        }

        public bool IsActive
        {
            get
            {
                return isActive;
            }
        }

        public IWebProxy WebProxy
        {
            get
            {
                return webProxy;
            }
            set
            {
                webProxy = value;
                loader.SetWebProxy(webProxy);
            }
        }

        #endregion
        public event Action<object> OnCompleted;
        public event Action<object, string> OnNewRequestResult;
        public event Action<object, string, string> OnNewRequestResultTable;

        /// <summary>
        /// Без установки прокси сервера(прямой переход)
        /// </summary>
        /// <param name="parser">Реализация парсера</param>
        /// <param name="htmlLoader">Реализация загрузчика</param>
        public ParserWorker(IParser<T> parser, IHtmlLoader htmlLoader)
        {
            this.parser = parser ?? throw new ArgumentException("parser");
            loader = htmlLoader ?? throw new ArgumentException("htmlLoader");
        }
        /// <summary>
        /// C установкой прокси сервера
        /// </summary>
        /// <param name="parser">Реализация парсера</param>
        /// <param name="htmlLoader">Реализация загрузчика</param>
        /// <param name="webProxy">Прокси сервер</param>
        public ParserWorker(IParser<T> parser, IHtmlLoader htmlLoader, IWebProxy webProxy) : this(parser, htmlLoader)
        {
            this.webProxy = webProxy;
            loader.SetWebProxy(webProxy);
        }


        public void Start(string currentUrl)
        {
            isActive = true;
            Worker(currentUrl);
        }
        public void Start(string currentUrl, string cellReference)
        {
            isActive = true;
            Worker(currentUrl, cellReference);
        }
        public void Start(List<string> currentUrls)
        {
            isActive = true;
            Worker(currentUrls);
        }

        public void Abort()
        {
            isActive = false;
        }

        private async void Worker(string currentUrl)
        {

            if (isUrl(currentUrl))
            {
                var source = await loader.GetResponseCodeByProxy(currentUrl);

                OnNewRequestResult?.Invoke(this, source);
            }
            else
            {
                OnNewRequestResult?.Invoke(this, "No address");
            }

            #region Тут все, для того щоб парсити сторінку (розкоментуй, якщо буде необхідно щось парсити)
            //var source = await loader.GetResponseText(currentUrl);

            //var domParser = new HtmlParser();

            //var document = await domParser.ParseAsync(source);

            //var result = parser.Parse(document);

            //OnNewData?.Invoke(this, result);

            #endregion


            OnCompleted?.Invoke(this);

            isActive = false;
        }

        /// <summary>
        /// Для роботи з таблицями
        /// </summary>
        /// <param name="currentUrl">посилання</param>
        /// <param name="cellReference">номер комірки таблиці</param>
        private async void Worker(string cellReference, string currentUrl )
        {
            if (isUrl(currentUrl))
            {
                var source = await loader.GetResponseCodeByProxy(currentUrl);

                OnNewRequestResultTable?.Invoke(this, cellReference, source);
            }
            else
            {
                OnNewRequestResultTable?.Invoke(this, cellReference, "No address");
            }

            #region Тут все, для того щоб парсити сторінку (розкоментуй, якщо буде необхідно щось парсити)
            //var source = await loader.GetResponseText(currentUrl);

            //var domParser = new HtmlParser();

            //var document = await domParser.ParseAsync(source);

            //var result = parser.Parse(document);

            //OnNewData?.Invoke(this, result);

            #endregion


            OnCompleted?.Invoke(this);

            isActive = false;
        }
        private async void Worker(List<string> urls)
        {
            foreach (var currentUrl in urls)
            {
                if (isUrl(currentUrl))
                {
                    var source = await loader.GetResponseCodeByProxy(currentUrl);

                    OnNewRequestResult?.Invoke(this, source);
                }
                else
                {
                    OnNewRequestResult?.Invoke(this, "No address");
                }

            }

            #region Тут все, для того щоб парсити сторінку (розкоментуй, якщо буде необхідно щось парсити)
            //var source = await loader.GetResponseText(currentUrl);

            //var domParser = new HtmlParser();

            //var document = await domParser.ParseAsync(source);

            //var result = parser.Parse(document);

            //OnNewData?.Invoke(this, result);

            #endregion

            OnCompleted?.Invoke(this);

            isActive = false;
        }

        private bool isUrl(string text)
        {
            string urlPattern = "http(s)?://([\\w+?\\.\\w+])+([a-zA-Z0-9\\~\\!\\@\\#\\$\\%\\^\\&\\*\\(\\)_\\-\\=\\+\\\\\\/\\?\\.\\:\\;\\\'\\,]*)?";

            return Regex.IsMatch(text, urlPattern);
        }

    }
}
