using AngleSharp.Dom.Html;

namespace Proxy.Parser
{
    interface IParser<T> where T : class
    {
        T Parse(IHtmlDocument document);
    }
}
