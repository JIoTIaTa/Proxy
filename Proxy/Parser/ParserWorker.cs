using System;
using System.Collections.Generic;
using System.Net;
using AngleSharp.Parser.Html;

namespace Proxy.Parser
{
    class ParserWorker<T> where T : class
    {
        IParser<T> parser;

        HtmlLoader loader;

        WebProxy webProxy;

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

        public WebProxy WebProxy
        {
            get
            {
                return webProxy;
            }
            set
            {
                webProxy = value;
                loader.WebProxy = webProxy;
            }
        }

        #endregion

        public event Action<object, T> OnNewData;
        public event Action<object> OnCompleted;
        public event Action<object, string> OnNewRequestResult;

        public ParserWorker(IParser<T> parser)
        {
            this.parser = parser;
            loader = new HtmlLoader();
        }

        public ParserWorker(IParser<T> parser, WebProxy webProxy) : this(parser)
        {
            this.webProxy = webProxy;
            loader.WebProxy = webProxy;
        }


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
            //var source = await loader.GetResponse(currentUrl);

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
            //var source = await loader.GetResponse(currentUrl);

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
