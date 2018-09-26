using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Proxy.Parser.Facebook;

namespace Proxy.Parser
{
    class HttpClientLoader : IHtmlLoader
    {

        IWebProxy webProxy;

        private readonly string accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8";
        private readonly string userAgent = "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/68.0.3440.106 Safari/537.36";
        private readonly string acceptCharset = "ISO-8859-1";
        private readonly string acceptEncoding = "gzip, deflate, br";

        //private readonly HttpClient httpClient;
        public IWebProxy WebProxy
        {
            get { return webProxy; }
            set { webProxy = value; }
        }

        public HttpClientLoader(IWebProxy webProxy)
        {
            this.webProxy = webProxy ??  throw new ArgumentException("webProxy load ERROR");
        }

        public HttpClientLoader()
        {
           
        }

        /// <summary>
        /// Установить прокси сервер
        /// </summary>
        /// <param name="webProxy"> прокси сервер</param>
        public void SetWebProxy(IWebProxy webProxy)
        {
            this.webProxy = webProxy;
        }
        /// <summary>
        /// Простое соединение со страницей
        /// </summary>
        /// <param name="currentUrl">Ссылка на страницу</param>
        /// <returns></returns>
        public async Task<string> Connect(string currentUrl)
        {
            //HttpClient httpClient = new HttpClient();
            string result = null;
            using (var httpClient = new HttpClient())
            {
                using (var response = await httpClient.GetAsync(currentUrl))
                {
                    if (response != null && response.StatusCode == HttpStatusCode.OK)
                    {
                        result = await response.Content.ReadAsStringAsync();
                    }
                    else
                    {
                        result = response.StatusCode.ToString();
                    }
                }
            }
            return result;
        }
        /// <summary>
        /// Получить страницу в виде строки
        /// </summary>
        /// <param name="url">Ссылка на страницу</param>
        /// <returns></returns>
        public async Task<string> GetResponseText(string url)
        {
            //HttpClient httpClient = new HttpClient();
            using (var request = new HttpRequestMessage(HttpMethod.Get, new Uri(url)))
            {
                request.Headers.TryAddWithoutValidation("Accept", accept);
                request.Headers.TryAddWithoutValidation("User-Agent", userAgent);
                request.Headers.TryAddWithoutValidation("Accept-Charset", acceptCharset);

                string result = null;
                using (var httpClient = new HttpClient())
                {
                    using (var response = await httpClient.SendAsync(request).ConfigureAwait(false))
                    {
                        if (response.StatusCode == HttpStatusCode.OK)
                        {
                            using (var responseStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false))
                            {
                                byte[] streamByte = new byte[responseStream.Length];
                                responseStream.Read(streamByte, 0, streamByte.Length);
                                result = Encoding.UTF8.GetString(streamByte);
                            }
                        }
                        else
                        {
                            result = $"{url} -> {response.StatusCode.ToString()}";
                        }
                        return result;
                    }
                }
            }
        }
        /// <summary>
        /// Получить код ответа сервера страницы
        /// </summary>
        /// <param name="url">Ссылка на страницу</param>
        /// <returns></returns>
        public async Task<string> GetResponseCode(string url)
        {
            string result;
            using (var httpClient = new HttpClient())
            {
                using (var request = new HttpRequestMessage(HttpMethod.Get, new Uri(url)))
                {
                    request.Headers.TryAddWithoutValidation("Accept", accept);
                    request.Headers.TryAddWithoutValidation("User-Agent", userAgent);
                    request.Headers.TryAddWithoutValidation("Accept-Charset", acceptCharset);

                    using (var response = await httpClient.SendAsync(request).ConfigureAwait(false))
                    {
                        result = $"{url} -> {response.StatusCode.ToString()}";
                        httpClient.Dispose();
                    }
                }
            }
            return result;
        }
        /// <summary>
        /// Получить код ответа сервера страницы через прокси-сервер
        /// </summary>
        /// <param name="url">Ссылка на страницу</param>
        /// <returns></returns>
        public async Task<string> GetResponseCodeByProxy(string url)
        {
            string result = null;
            try
            {
                HttpClientHandler httpClientHandler = new HttpClientHandler();
                httpClientHandler.Proxy = webProxy;
                using (var httpClient1 = new HttpClient(httpClientHandler))
                {
                    using (var request = new HttpRequestMessage(HttpMethod.Get, new Uri(url)))
                    {
                        request.Headers.TryAddWithoutValidation("Accept", accept);
                        request.Headers.TryAddWithoutValidation("User-Agent", userAgent);
                        request.Headers.TryAddWithoutValidation("Accept-Charset", acceptCharset);

                        using (var response = await httpClient1.SendAsync(request).ConfigureAwait(false))
                        {
                            result = $"{url} -> {response.StatusCode.ToString()}";
                            return result;
                        }
                    }
                }
               
            }
            catch (Exception e)
            {
                result = $"{url} -> {e.Message}";
                return result;
            }
        }

    }
}
