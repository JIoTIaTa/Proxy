using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Ninject;
using Proxy.Parser.Facebook;

namespace Proxy.Parser
{
    class HttpClientLoader : IHtmlLoader
    {

        IWebProxy webProxy;
        private readonly HttpClient httpClient;
        public IWebProxy WebProxy
        {
            get { return webProxy; }
            set { webProxy = value; }
        }


        public HttpClientLoader(HttpClient httpClient)
        {
            if(httpClient == null)
                throw new ArgumentException("httpClient");
            this.httpClient = httpClient;
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

            var response = await httpClient.GetAsync(currentUrl);

            string source = null;

            if (response != null && response.StatusCode == HttpStatusCode.OK)
            {
                 source = await response.Content.ReadAsStringAsync();
            }

            httpClient.Dispose();
            return source;
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
                request.Headers.TryAddWithoutValidation("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8");
                //request.Headers.TryAddWithoutValidation("Accept-Encoding", "gzip, deflate, br");
                request.Headers.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/68.0.3440.106 Safari/537.36");
                request.Headers.TryAddWithoutValidation("Accept-Charset", "ISO-8859-1");

                string result = null;

                using (var response = await httpClient.SendAsync(request).ConfigureAwait(false))
                {
                    //response.EnsureSuccessStatusCode();
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        using (var responseStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false))
                        {
                            //result = await responseStream.ReadAsync()
                            //var stream = await response.Content.ReadAsStreamAsync();

                            byte[] streamByte = new byte[responseStream.Length];
                            responseStream.Read(streamByte, 0, streamByte.Length);
                            result = Encoding.UTF8.GetString(streamByte);
                        }
                        //using (var decompressedStream = new GZipStream(responseStream, CompressionMode.Decompress))
                        //using (var streamReader = new StreamReader(decompressedStream))
                        //{
                        //    string result1 = await streamReader.ReadToEndAsync().ConfigureAwait(false);
                        //}
                    }
                    else
                    {
                        result = $"{url} -> {response.StatusCode.ToString()}";
                    }
                    httpClient.Dispose();
                    return result;
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
            //HttpClient httpClient = new HttpClient();
            
            using (var request = new HttpRequestMessage(HttpMethod.Get, new Uri(url)))
            {
                request.Headers.TryAddWithoutValidation("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8");
                //request.Headers.TryAddWithoutValidation("Accept-Encoding", "gzip, deflate, br");
                request.Headers.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/68.0.3440.106 Safari/537.36");
                request.Headers.TryAddWithoutValidation("Accept-Charset", "ISO-8859-1");

                using (var response = await httpClient.SendAsync(request).ConfigureAwait(false))
                {
                    var result = $"{url} -> {response.StatusCode.ToString()}";
                    httpClient.Dispose();
                    return result;
                }
            }
        }
        /// <summary>
        /// Получить код ответа сервера страницы через прокси-сервер
        /// </summary>
        /// <param name="url">Ссылка на страницу</param>
        /// <returns></returns>
        public async Task<string> GetResponseCodeByProxy(string url)
        {
            //HttpClient httpClient = null;

            //HttpClientHandler httpClientHandler = new HttpClientHandler();

            //httpClientHandler.Proxy = webProxy;

            //httpClient = new HttpClient(httpClientHandler);

            string result = null;
            try
            {
                using (var request = new HttpRequestMessage(HttpMethod.Get, new Uri(url)))
                {
                    request.Headers.TryAddWithoutValidation("Accept",
                        "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8");
                    //request.Headers.TryAddWithoutValidation("Accept-Encoding", "gzip, deflate, br");
                    request.Headers.TryAddWithoutValidation("User-Agent",
                        "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/68.0.3440.106 Safari/537.36");
                    request.Headers.TryAddWithoutValidation("Accept-Charset", "ISO-8859-1");

                    using (var response = await httpClient.SendAsync(request).ConfigureAwait(false))
                    {
                        result = $"{url} -> {response.StatusCode.ToString()}";
                        return result;
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
