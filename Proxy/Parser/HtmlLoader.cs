using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Proxy.Parser
{
    class HtmlLoader
    {
        
        WebProxy webProxy;

        public WebProxy WebProxy
        {
            get { return webProxy; }
            set { webProxy = value; }
        }

        public HtmlLoader()
        {
                
        }
        /// <summary>
        /// Черех прокси сервер
        /// </summary>
        /// <param name="webProxy"></param>
        public HtmlLoader(WebProxy webProxy)
        {
            this.webProxy = webProxy;
        }


        public async Task<string> GetSourceByUrl(string currentUrl)
        {
            HttpClient client = new HttpClient();

            var response = await client.GetAsync(currentUrl);

            string source = null;

            if (response != null && response.StatusCode == HttpStatusCode.OK)
            {
                 source = await response.Content.ReadAsStringAsync();
            }

            client.Dispose();
            return source;
        }
        public async Task<string> GetResponse(string url)
        {
            HttpClient httpClient = new HttpClient();
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

        public async Task<string> GetResponseCode(string url)
        {
            HttpClient httpClient = new HttpClient();
            
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

        public async Task<string> GetResponseCodeByProxy(string url)
        {
            HttpClient httpClient = null;

            HttpClientHandler httpClientHandler = new HttpClientHandler();

            httpClientHandler.Proxy = webProxy;

            httpClient = new HttpClient(httpClientHandler);

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
                        httpClient.Dispose();
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
