using BenchmarkDotNet.Attributes;
using HtmlAgilityPack;
using System.Text;
using System.Xml.XPath;

namespace DmrScraper.Benchmarks;

[MemoryDiagnoser(false)]
public class HtmlReadBenchmarks
{
    private HtmlDocument _technicalInformationHtml;
    private HtmlDocument _insuranceHtml;

    [GlobalSetup]
    public void Setup()
    {
        //using var fs = File.OpenRead(@".\DMR - Køretøj.html");

        _technicalInformationHtml = LoadHtmlFile(@".\DMR - Tekniske oplysninger.html");
        _insuranceHtml = LoadHtmlFile(@".\DMR - Forsikring.html");
    }

    private static HtmlDocument LoadHtmlFile(string path)
    {
        using var fs = File.OpenRead(path);

        var htmlDocument = new HtmlDocument();
        htmlDocument.Load(fs);

        return htmlDocument;
    }

    [Benchmark]
    public List<KeyValuePair<string, string>> NextTechnical()
    {
        var reader = new DmrHtmlReaderNext(_technicalInformationHtml, DmrHtmlReaderNext.ReadStrategy.InsideFieldGroup);

        return reader.ReadKeyValuePairs(includeEmpty: false, includeFalse: true).ToList();
    }

    [Benchmark]
    public List<KeyValuePair<string, string>> CurrentTechnical()
    {
        var reader = new DmrHtmlReaderCurrent(_technicalInformationHtml);

        return reader.ReadKeyValuePairs(includeEmpty: false, includeFalse: true);
    }

    [Benchmark]
    public List<KeyValuePair<string, string>> NextInsurance()
    {
        var reader = new DmrHtmlReaderNext(_insuranceHtml, DmrHtmlReaderNext.ReadStrategy.InsideContent);

        return reader.ReadKeyValuePairs(includeEmpty: false, includeFalse: true).ToList();
    }

    [Benchmark]
    public List<KeyValuePair<string, string>> CurrentInsurance()
    {
        var reader = new DmrHtmlReaderCurrent(_insuranceHtml);

        return reader.ReadKeyValuePairs(includeEmpty: false, includeFalse: true);
    }
}

internal static class HtmlNodeExtensions
{
    public static IEnumerable<HtmlNode> SelectNodesOrEmpty(this HtmlNode node, XPathExpression xpath)
    {
        return node.SelectNodes(xpath) ?? Enumerable.Empty<HtmlNode>();
    }
}

internal class DmrHtmlReaderNext(HtmlNode contentNode, DmrHtmlReaderNext.ReadStrategy readStrategy)
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

    public enum ReadStrategy
    {
        InsideFieldGroup,
        InsideContent
    }

    private readonly HtmlNode? _contentNode = contentNode;
    private readonly ReadStrategy _readStrategy = readStrategy;

    public bool HasContent => _contentNode != null;

    public DmrHtmlReaderNext(HtmlDocument htmlDocument, ReadStrategy readStrategy)
        : this(htmlDocument.DocumentNode.SelectSingleNode(XPaths.Content), readStrategy)
    {
    }

    public IEnumerable<KeyValuePair<string, string>> ReadKeyValuePairs(bool includeEmpty, bool includeFalse)
    {
        if (_contentNode == null)
            return [];

        return _readStrategy switch
        {
            ReadStrategy.InsideFieldGroup => ReadKeyValuePairsFromFieldGroup(_contentNode, includeEmpty, includeFalse),
            ReadStrategy.InsideContent => ReadKeyValuePairsFromContent(_contentNode, includeEmpty, includeFalse),
            _ => throw new ArgumentOutOfRangeException(nameof(_readStrategy), _readStrategy, null),
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

internal class DmrHtmlReaderCurrent(HtmlNode contentNode)
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

    public DmrHtmlReaderCurrent(HtmlDocument htmlDocument)
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