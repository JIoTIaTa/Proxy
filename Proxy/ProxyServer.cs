using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace Proxy
{
    class ProxyServer
    {
        private string iPAddress;
        private int port;
        private string login;
        private string password;
        WebProxy webProxy;

        public string IPAddress
        {
            get { return iPAddress; }
            set { iPAddress = value; }
        }
        public string Login
        {
            get { return login; }
            set { login = value; }
        }
        public int Port
        {
            get { return port; }
            set { port = value; }
        }
        public string Password
        {
            get { return password; }
            set { password = value; }
        }

        public ProxyServer(string address, int port, string login, string password)
        {
            this.iPAddress = address;
            this.port = port;
            this.login = login;
            this.password = password;
        }
        public WebProxy Create()
        {
            try
            {
                webProxy = new WebProxy(iPAddress, port);
                webProxy.Credentials = new NetworkCredential(login, password);
                return webProxy;
            }
            catch (Exception)
            {
                return null;
            }            

        }
        
    }
}