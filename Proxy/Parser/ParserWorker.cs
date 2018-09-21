using System;
using System.Collections.Generic;
using System.Net;
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

        public event Action<object, T> OnNewData;
        public event Action<object> OnCompleted;
        public event Action<object, string> OnNewRequestResult;

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
        //public ParserWorker(IParser<T> parser, IHtmlLoader htmlLoader, IWebProxy webProxy) : this(parser, htmlLoader)
        //{
        //    this.webProxy = webProxy;
        //    loader.SetWebProxy(webProxy);
        //}


        public void Start(string currentUrl)
        {
            isActive = true;
            Worker(currentUrl);
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

            if (currentUrl != null)
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
        private async void Worker(List<string> currentUrls)
        {
            foreach (var url in currentUrls)
            {
                if (url != null)
                {
                    var source = await loader.GetResponseCodeByProxy(url);

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


    }
}
