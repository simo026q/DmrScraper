using BenchmarkDotNet.Attributes;
using HtmlAgilityPack;
using System.Text;
using System.Xml.XPath;

namespace DmrScraper.Benchmarks;

[MemoryDiagnoser(false)]
public class HtmlReadBenchmarks
{
    private HtmlDocument _htmlDocument;

    [GlobalSetup]
    public void Setup()
    {
        //using var fs = File.OpenRead(@".\DMR - Køretøj.html");
        using var fs = File.OpenRead(@".\DMR - Tekniske oplysninger.html");

        _htmlDocument = new HtmlDocument();
        _htmlDocument.Load(fs);
    }

    [Benchmark]
    public List<KeyValuePair<string, string>> Next()
    {
        var reader = new DmrHtmlReaderNext(_htmlDocument);

        return reader.ReadKeyValuePairs(includeEmpty: false, includeFalse: true);
    }

    [Benchmark]
    public List<KeyValuePair<string, string>> Current()
    {
        var reader = new DmrHtmlReaderCurrent(_htmlDocument);

        return reader.ReadKeyValuePairs(includeEmpty: false, includeFalse: true);
    }
}

internal class DmrHtmlReaderNext(HtmlNode contentNode)
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

    public DmrHtmlReaderNext(HtmlDocument htmlDocument)
        : this(htmlDocument.DocumentNode.SelectSingleNode(XPaths.Content))
    {
    }

    public List<KeyValuePair<string, string>> ReadKeyValuePairs(bool includeEmpty, bool includeFalse)
    {
        return ReadKeyValuePairsFromHtmlNode(_contentNode, includeEmpty, includeFalse);
    }

    private static List<KeyValuePair<string, string>> ReadKeyValuePairsFromHtmlNode(HtmlNode htmlNode, bool includeUnknown, bool includeFalse)
    {
        var keyValuePairs = new List<KeyValuePair<string, string>>();

        var fieldGroupNodes = htmlNode.SelectNodes(XPaths.FieldGroup);
        if (fieldGroupNodes == null)
            return keyValuePairs;

        foreach (var fieldGroupNode in fieldGroupNodes)
        {
            var headerNode = fieldGroupNode.SelectSingleNode(XPaths.FieldGroupHeader);
            string? header = headerNode?.InnerText.Trim();

            var keyValueDivs = fieldGroupNode.SelectNodes(XPaths.KeyValueContainer);
            if (keyValueDivs != null)
            {
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

                    keyValuePairs.Add(new KeyValuePair<string, string>(keyBuilder.ToString(), value));
                }
            }

            var lineDivs = fieldGroupNode.SelectNodes(XPaths.Line);
            if (lineDivs != null)
            {
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

                    keyValuePairs.Add(new KeyValuePair<string, string>(keyBuilder.ToString(), value));
                }
            }
        }

        return keyValuePairs;
    }
}

internal class DmrHtmlReaderCurrent(HtmlNode contentNode)
{
    private readonly HtmlNode _contentNode = contentNode;

    public DmrHtmlReaderCurrent(HtmlDocument htmlDocument)
        : this(htmlDocument.DocumentNode.SelectSingleNode("//div[@class='h-tab-content-inner']"))
    {
    }

    public List<KeyValuePair<string, string>> ReadKeyValuePairs(bool includeEmpty, bool includeFalse)
    {
        return ReadKeyValuePairsFromHtmlNode(_contentNode, includeEmpty, includeFalse);
    }

    private static List<KeyValuePair<string, string>> ReadKeyValuePairsFromHtmlNode(HtmlNode htmlNode, bool includeUnknown, bool includeFalse)
    {
        var keyValuePairs = new List<KeyValuePair<string, string>>();

        var fieldGroupNodes = htmlNode.SelectNodes("./div[@class='fieldGroup']");
        if (fieldGroupNodes != null)
        {
            foreach (var fieldGroupNode in fieldGroupNodes)
            {
                var headerNode = fieldGroupNode.SelectSingleNode("./h3[@class='fieldGroupHeader']");
                string? header = headerNode?.InnerText.Trim();

                var keyValueDivs = fieldGroupNode.SelectNodes(".//div[contains(@class,'keyvalue')]");
                if (keyValueDivs != null)
                {
                    foreach (var div in keyValueDivs)
                    {
                        var keyNode = div.SelectSingleNode("./span[@class='key']");
                        var valueNode = div.SelectSingleNode("./span[@class='value']");

                        if (keyNode != null && valueNode != null)
                        {
                            var key = header == null
                                ? keyNode.InnerText.Trim().TrimEnd(':')
                                : $"{header}.{keyNode.InnerText.Trim().TrimEnd(':')}";

                            var value = valueNode.InnerText.Trim();

                            if (string.IsNullOrWhiteSpace(value)
                                || !includeUnknown && value == "-"
                                || !includeFalse && value == "Nej")
                            {
                                continue;
                            }

                            keyValuePairs.Add(new KeyValuePair<string, string>(key, value));
                        }
                    }
                }

                var lineDivs = fieldGroupNode.SelectNodes(".//div[contains(@class,'line')]");
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

                                keyValuePairs.Add(new KeyValuePair<string, string>(keyBuilder.ToString(), value));
                            }
                            else
                            {
                                lastNonIndentedKey = key;
                            }
                        }
                    }
                }
            }
        }

        return keyValuePairs;
    }
}