using HtmlAgilityPack;
using System.Text;
using System.Xml.XPath;

namespace DmrScraper.Internal;

internal sealed class DmrHtmlReader(HtmlNode contentNode, DmrPage page)
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

    private readonly HtmlNode? _contentNode = contentNode;
    private readonly DmrPage _page = page;

    public bool HasContent => _contentNode != null;

    public DmrHtmlReader(HtmlDocument htmlDocument, DmrPage page)
        : this(htmlDocument.DocumentNode.SelectSingleNode(XPaths.Content), page)
    {
    }

    public IEnumerable<KeyValuePair<string, string>> ReadKeyValuePairs(bool includeEmpty, bool includeFalse)
    {
        if (_contentNode == null)
            return [];

        return _page switch
        {
            DmrPage.Vehicle or DmrPage.Technical => ReadKeyValuePairsFromFieldGroup(_contentNode, includeEmpty, includeFalse),
            DmrPage.Inspection or DmrPage.Insurance => ReadKeyValuePairsFromContent(_contentNode, includeEmpty, includeFalse),
            _ => throw new InvalidOperationException($"Specified DmrPage value is not supported in this class"),
        };
    }

    private static IEnumerable<KeyValuePair<string, string>> ReadKeyValuePairsFromFieldGroup(HtmlNode htmlNode, bool includeUnknown, bool includeFalse)
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

                if (!IsValueValid(value, includeUnknown, includeFalse))
                {
                    continue;
                }

                string key = keyNode.InnerText.Trim().TrimEnd(':');

                if (header != null)
                {
                    StringBuilder keyBuilder = new();

                    keyBuilder.Append(header);
                    keyBuilder.Append('.');
                    keyBuilder.Append(key);

                    key = keyBuilder.ToString();
                }

                yield return new KeyValuePair<string, string>(key, value);
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

                if (!IsValueValid(value, includeUnknown, includeFalse))
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

    private static IEnumerable<KeyValuePair<string, string>> ReadKeyValuePairsFromContent(HtmlNode htmlNode, bool includeUnknown, bool includeFalse)
    {
        var keyValuePairs = ReadKeyValuePairsFromNodeBySelectors(htmlNode, XPaths.KeyValueContainer, XPaths.KeyValueKey, XPaths.KeyValueValue, includeUnknown, includeFalse);
        foreach (var pair in keyValuePairs)
        {
            yield return pair;
        }

        var linePairs = ReadKeyValuePairsFromNodeBySelectors(htmlNode, XPaths.Line, XPaths.LineKey, XPaths.LineValue, includeUnknown, includeFalse);
        foreach (var pair in linePairs)
        {
            yield return pair;
        }
    }

    private static IEnumerable<KeyValuePair<string, string>> ReadKeyValuePairsFromNodeBySelectors(HtmlNode rootNode, 
        XPathExpression containerExpression, 
        XPathExpression keyExpression, 
        XPathExpression valueExpression, 
        bool includeUnknown, 
        bool includeFalse)
    {
        var keyValueNodes = rootNode.SelectNodesOrEmpty(containerExpression);

        foreach (var keyValueNode in keyValueNodes)
        {
            var keyNode = keyValueNode.SelectSingleNode(keyExpression);
            var valueNode = keyValueNode.SelectSingleNode(valueExpression);

            if (keyNode == null || valueNode == null)
                continue;

            var value = valueNode.InnerText.Trim();

            if (!IsValueValid(value, includeUnknown, includeFalse))
            {
                continue;
            }

            var key = keyNode.InnerText.Trim().TrimEnd(':');

            yield return new KeyValuePair<string, string>(key, value);
        }
    }

    private static bool IsValueValid(string value, bool includeUnknown, bool includeFalse)
    {
        return !string.IsNullOrWhiteSpace(value)
            && (includeUnknown || value != "-")
            && (includeFalse || value != "Nej");
    }

    private static string? GetFormGroupHeader(HtmlNode fieldGroupNode)
    {
        var headerNode = fieldGroupNode.SelectSingleNode(XPaths.FieldGroupHeader);
        return headerNode?.InnerText.Trim();
    }
}
