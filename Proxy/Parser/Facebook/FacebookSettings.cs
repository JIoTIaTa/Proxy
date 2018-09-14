using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Proxy.Parser.Facebook
{
    class FacebookSettings : IParserSettings
    {
        public string BaseUrl { get; set; } = "https://www.facebook.com";
        public string Prefix { get; set; }
    }
}
