using HtmlAgilityPack;
using System.Text;
using System.Xml.XPath;

namespace DmrScraper.Internal;

internal class DmrHtmlReader(HtmlNode contentNode)
{
    private static class XPaths
    {
        public static readonly XPathExpression Content = XPathExpression.Compile("//div[@class='h-tab-content-inner']");
        public static readonly XPathExpression FieldGroup = XPathExpression.Compile("./div[@class='fieldGroup']");
        public static readonly XPathExpression FieldGroupHeader = XPathExpression.Compile("./h3[@class='fieldGroupHeader']");
        public static readonly XPathExpression KeyValueContainer = XPathExpression.Compile(".//div[contains(@class,'keyvalue')]");
        public static readonly XPathExpression KeyValueKey = XPathExpression.Compile("./span[@class='key']");
        public static readonly XPathExpression KeyValueValue = XPathExpression.Compile("./span[@class='value']");
        public static readonly XPathExpression Line = XPathExpression.Compile(".//div[contains(@class,'line')]");
        public static readonly XPathExpression LineKey = XPathExpression.Compile("./div[contains(@class,'colLabel')]//label");
        public static readonly XPathExpression LineValue = XPathExpression.Compile("./div[contains(@class,'colValue')]/span");
    }

    private readonly HtmlNode _contentNode = contentNode;

    public DmrHtmlReader(HtmlDocument htmlDocument)
        : this(htmlDocument.DocumentNode.SelectSingleNode(XPaths.Content))
    {
    }

    public IEnumerable<KeyValuePair<string, string>> ReadKeyValuePairs(bool includeEmpty, bool includeFalse)
    {
        return ReadKeyValuePairsFromHtmlNode(_contentNode, includeEmpty, includeFalse);
    }

    private static IEnumerable<KeyValuePair<string, string>> ReadKeyValuePairsFromHtmlNode(HtmlNode htmlNode, bool includeUnknown, bool includeFalse)
    {
        var fieldGroupNodes = htmlNode.SelectNodesOrEmpty(XPaths.FieldGroup);

        foreach (var fieldGroupNode in fieldGroupNodes)
        {
            string? header = GetFormGroupHeader(fieldGroupNode);

            var keyValueDivs = fieldGroupNode.SelectNodesOrEmpty(XPaths.KeyValueContainer);
            foreach (var div in keyValueDivs)
            {
                var keyNode = div.SelectSingleNode(XPaths.KeyValueKey);
                var valueNode = div.SelectSingleNode(XPaths.KeyValueValue);

                if (keyNode == null || valueNode == null)
                    continue;

                var value = valueNode.InnerText.Trim();

                if (string.IsNullOrWhiteSpace(value)
                    || !includeUnknown && value == "-"
                    || !includeFalse && value == "Nej")
                {
                    continue;
                }

                StringBuilder keyBuilder = new();

                if (header != null)
                {
                    keyBuilder.Append(header);
                    keyBuilder.Append('.');
                }

                var key = keyNode.InnerText.Trim().TrimEnd(':');

                keyBuilder.Append(key);

                yield return new KeyValuePair<string, string>(keyBuilder.ToString(), value);
            }

            var lineDivs = fieldGroupNode.SelectNodesOrEmpty(XPaths.Line);
            string? lastNonIndentedKey = null;

            foreach (var div in lineDivs)
            {
                var keyNode = div.SelectSingleNode(XPaths.LineKey);
                if (keyNode == null)
                    continue;

                var key = keyNode.InnerText.Trim().TrimEnd(':');

                var valueNode = div.SelectSingleNode(XPaths.LineValue);

                if (valueNode == null)
                {
                    lastNonIndentedKey = key;
                    continue;
                }

                var value = valueNode.InnerText.Trim();

                if (string.IsNullOrWhiteSpace(value)
                    || !includeUnknown && value == "-"
                    || !includeFalse && value == "Nej")
                {
                    continue;
                }

                StringBuilder keyBuilder = new();

                if (header != null)
                {
                    keyBuilder.Append(header);
                    keyBuilder.Append('.');
                }

                var isIndented = keyNode.ParentNode.GetAttributeValue("class", string.Empty) == "indented";
                if (isIndented && lastNonIndentedKey != null)
                {
                    keyBuilder.Append(lastNonIndentedKey);
                    keyBuilder.Append('.');
                }
                else
                {
                    lastNonIndentedKey = key;
                }

                keyBuilder.Append(key);

                yield return new KeyValuePair<string, string>(keyBuilder.ToString(), value);
            }
        }
    }

    private static string? GetFormGroupHeader(HtmlNode fieldGroupNode)
    {
        var headerNode = fieldGroupNode.SelectSingleNode(XPaths.FieldGroupHeader);
        return headerNode?.InnerText.Trim();
    }
}
