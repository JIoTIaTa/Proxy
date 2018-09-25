using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Ninject.Activation;
using Ninject.Modules;
using Ninject.Planning;
using Proxy.BookWorker;
using Proxy.GoogleDriveAPI;
using Proxy.Parser;
using Proxy.Parser.Facebook;

namespace Proxy.Ninject
{
    /// <summary>
    /// Впровадження залежностей для ParserWorker
    /// </summary>
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
            //Bind<HttpClient>().ToMethod(context => new HttpClient(new HttpClientHandler
            //    { Proxy = new ProxyServer(address, port, login, password).Create() }));
            Bind<IWebProxy>().ToMethod(context => createWebProxy());
            Bind<IParser<string[]>>().To<FacebookParser>();
            Bind<IHtmlLoader>().To<HttpClientLoader>();

            Bind<ParserWorker<string[]>>().ToSelf();
        }

        private IWebProxy createWebProxy()
        {
            WebProxy webProxy = new WebProxy(address, port);
            webProxy.Credentials = new NetworkCredential(login, password);
            return webProxy;
        }
    }

    /// <summary>
    /// Впровадження залежностей для Form1
    /// </summary>
    class FormLoadDIConfig : NinjectModule
    {
        private readonly string _formSettingsFullPath;
        private readonly string _clients_secretFullPath;


        public FormLoadDIConfig(string settingsFileName, string clients_secretFileName)
        {

            string userDocumentsPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            _formSettingsFullPath = Path.Combine(userDocumentsPath, "Proxy Master", settingsFileName);
            _clients_secretFullPath = Path.Combine(userDocumentsPath, "Proxy Master", clients_secretFileName);
        }
        public override void Load()
        {
            Bind<SerialazebleParametrs>().ToMethod(context =>
                readBinaryFile(_formSettingsFullPath));
            Bind<IBookWorker>().To<SpreadShetsWorker>();
            Bind<GDrive>().ToMethod(context => new GDrive(_clients_secretFullPath));
            Bind<Form1>().ToSelf();
        }
        /// <summary>
        /// Десеріалізуємо файл, або встановлюємо дефотні налаштування
        /// </summary>
        /// <param name="formSettingsFullPath"></param>
        /// <returns></returns>
        private SerialazebleParametrs readBinaryFile(string formSettingsFullPath)
        {
            if (Serializator.Read<SerialazebleParametrs>(formSettingsFullPath) != null)
            {
                return Serializator.Read<SerialazebleParametrs>(formSettingsFullPath);
            }
            else
            {
                return new SerialazebleParametrs();
            }
        }
    }
}
