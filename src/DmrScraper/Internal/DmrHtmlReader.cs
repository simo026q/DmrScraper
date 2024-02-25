using HtmlAgilityPack;

namespace DmrScraper.Internal;

internal class DmrHtmlReader(HtmlNode contentNode)
{
    private readonly HtmlNode _contentNode = contentNode;

    public DmrHtmlReader(HtmlDocument htmlDocument)
        : this(htmlDocument.DocumentNode.SelectSingleNode("//div[@class='h-tab-content-inner']"))
    {
    }

    public List<KeyValuePair<string, string>> ReadKeyValuePairs(bool includeEmpty, bool includeFalse)
    {
        return ReadKeyValuePairsFromHtmlNode(_contentNode, includeEmpty, includeFalse);
    }

    private static List<KeyValuePair<string, string>> ReadKeyValuePairsFromHtmlNode(HtmlNode htmlNode, bool includeEmpty, bool includeFalse)
    {
        var keyValuePairs = new List<KeyValuePair<string, string>>();

        void AddChecked(HtmlNode keyNode, HtmlNode valueNode)
        {
            if (keyNode != null && valueNode != null)
            {
                var key = keyNode.InnerText.Trim().TrimEnd(':');
                var value = valueNode.InnerText.Trim();

                if (!includeEmpty && (string.IsNullOrWhiteSpace(value) || value == "-"))
                {
                    return;
                }

                if (!includeFalse && value == "Nej")
                {
                    return;
                }

                keyValuePairs.Add(new KeyValuePair<string, string>(key, value));
            }
        }

        var keyValueDivs = htmlNode.SelectNodes(".//div[contains(@class,'keyvalue')]");
        if (keyValueDivs != null)
        {
            foreach (var div in keyValueDivs)
            {
                var keyNode = div.SelectSingleNode("./span[@class='key']");
                var valueNode = div.SelectSingleNode("./span[@class='value']");

                AddChecked(keyNode, valueNode);
            }
        }

        var lineDivs = htmlNode.SelectNodes(".//div[contains(@class,'line') and @id!='lblHstrskVsnngLine']");
        if (lineDivs != null)
        {
            foreach (var div in lineDivs)
            {
                var keyNode = div.SelectSingleNode("./div[contains(@class,'colLabel')]/label");
                var valueNode = div.SelectSingleNode("./div[contains(@class,'colValue')]/span");

                AddChecked(keyNode, valueNode);
            }
        }

        return keyValuePairs;
    }
}
