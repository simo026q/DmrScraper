using HtmlAgilityPack;
using System.Xml.XPath;

namespace DmrScraper.Internal;

internal static class HtmlNodeExtensions
{
    public static IEnumerable<HtmlNode> SelectNodesOrEmpty(this HtmlNode node, XPathExpression xpath)
    {
        return node.SelectNodes(xpath) ?? Enumerable.Empty<HtmlNode>();
    }
}
