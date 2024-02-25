using HtmlAgilityPack;
using System.Diagnostics;
using System.Reflection.Emit;

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

        var keyValueDivs = htmlNode.SelectNodes(".//div[contains(@class,'keyvalue')]");
        if (keyValueDivs != null)
        {
            foreach (var div in keyValueDivs)
            {
                var keyNode = div.SelectSingleNode("./span[@class='key']");
                var valueNode = div.SelectSingleNode("./span[@class='value']");

                if (keyNode != null && valueNode != null)
                {
                    var key = keyNode.InnerText.Trim().TrimEnd(':');
                    var value = valueNode.InnerText.Trim();

                    if (!includeEmpty && (string.IsNullOrWhiteSpace(value) || value == "-"))
                    {
                        continue;
                    }

                    if (!includeFalse && value == "Nej")
                    {
                        continue;
                    }

                    keyValuePairs.Add(new KeyValuePair<string, string>(key, value));
                }
            }
        }

        var lineDivs = htmlNode.SelectNodes(".//div[contains(@class,'line') and (@id!='lblHstrskVsnngLine' or not(@id))]");
        if (lineDivs != null)
        {
            string? lastNonIndentedKey = null;

            foreach (var div in lineDivs)
            {
                var keyNode = div.SelectSingleNode("./div[contains(@class,'colLabel')]//label");

                if (keyNode != null)
                {
                    var key = keyNode.InnerText.Trim().TrimEnd(':');

                    var valueNode = div.SelectSingleNode("./div[contains(@class,'colValue')]/span");

                    if (valueNode != null)
                    {
                        var value = valueNode.InnerText.Trim();

                        if (!includeEmpty && (string.IsNullOrWhiteSpace(value) || value == "-"))
                        {
                            continue;
                        }

                        if (!includeFalse && value == "Nej")
                        {
                            continue;
                        }

                        var isIndented = keyNode.ParentNode.GetAttributeValue("class", string.Empty) == "indented";
                        if (isIndented && lastNonIndentedKey != null)
                        {
                            key = $"{lastNonIndentedKey}.{key}";
                        }
                        else
                        {
                            lastNonIndentedKey = key;
                        }

                        var fieldGroupAncestor = div.SelectSingleNode("./ancestor::div[@class='fieldGroup']/h3[@class='fieldGroupHeader']");
                        if (fieldGroupAncestor != null)
                        {
                            var header = fieldGroupAncestor.InnerText.Trim();
                            key = $"{header}.{key}";
                        }

                        keyValuePairs.Add(new KeyValuePair<string, string>(key, value));
                    }
                    else
                    {
                        lastNonIndentedKey = key;
                    }
                }
            }
        }

        return keyValuePairs;
    }
}
