using System;
using System.Collections.Generic;
using System.Linq;
using AngleSharp.Dom.Html;

namespace Proxy.Parser.Facebook
{
    class FacebookParser : IParser<string[]>
    {

        public string[] Parse(IHtmlDocument document)
        {
            var list = new List<string>();
            //var items = document.QuerySelectorAll("h2").Where(item => item.ClassName != null && item.ClassName.Contains("_4-dq"));
            var items = document.QuerySelectorAll("h2").Where(item => item.ClassName != null);

            foreach (var item in items)
            {
                list.Add(item.TextContent);
            }

            return list.ToArray();
        }
    }
}
