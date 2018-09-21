using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Ninject.Modules;
using Proxy.Parser;
using Proxy.Parser.Facebook;

namespace Proxy.Ninject
{
    class ParserNjConfig : NinjectModule
    {
        private readonly string address;
        private readonly int port;
        private readonly string login;
        private readonly string password;

        public ParserNjConfig(string address, int port, string login, string password)
        {
            this.address = address;
            this.port = port;
            this.login = login;
            this.password = password;
        }
        public override void Load()
        {
            //Bind<IWebProxy>().ToMethod(context => new ProxyServer(address, port, login, password).Create());
            Bind<IParser<string[]>>().To<FacebookParser>();
            Bind<IHtmlLoader>().To<HttpClientLoader>();
            Bind<ParserWorker<string[]>>().ToSelf();
            Bind<HttpClient>().ToMethod(context => new HttpClient(new HttpClientHandler
            { Proxy = new ProxyServer(address, port, login, password).Create() }));
        }
    }
}
