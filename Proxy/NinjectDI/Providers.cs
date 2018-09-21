using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Ninject.Activation;

namespace Proxy.NinjectDI
{
    public interface IProvider
    {
        Type Type { get; }
        object Create(IContext context);
    }
    public abstract class Provider<T> : IProvider
    {
        protected Provider() {}
        public object Create(IContext context) { return context; }
        public Type Type { get; }
        protected abstract T CreateInstanse(IContext context);
    }

    public class HttpClientProvider : Provider<HttpClient>
    {
        private string address;
        private int port;
        private string login;
        private string password;
        public HttpClientProvider(string address, int port, string login, string password)
        {
            this.address = address;
            this.port = port;
            this.login = login;
            this.password = password;
        }
        protected override HttpClient CreateInstanse(IContext context)
        {
            HttpClientHandler handler = new HttpClientHandler();
            handler.Proxy = new WebProxy(address, port) { Credentials = new NetworkCredential(login, password) };
            HttpClient httpClient = new HttpClient();
            return httpClient;
        }
    }
}
