using BenchmarkDotNet.Attributes;
using HtmlAgilityPack;

namespace DmrScraper.Benchmarks;

[MemoryDiagnoser(false)]
public class HtmlReadBenchmarks
{
    private HtmlDocument _htmlDocument;

    [GlobalSetup]
    public void Setup()
    {
        using var fs = File.OpenRead(@".\Vis køretøj - DMR Kerne.html");

        _htmlDocument = new HtmlDocument();
        _htmlDocument.Load(fs);
    }

    [Benchmark]
    public Dictionary<string, string> Version1()
    {
        var dictionary = new Dictionary<string, string>();

        var contentNode = _htmlDocument.DocumentNode.SelectSingleNode("//div[@class='h-tab-content-inner']");

        var keyValueDivs = contentNode.SelectNodes("//div[contains(@class,'keyvalue')]");
        if (keyValueDivs != null)
        {
            foreach (var div in keyValueDivs)
            {
                var keyNode = div.SelectSingleNode(".//span[@class='key']");
                var valueNode = div.SelectSingleNode(".//span[@class='value']");

                if (keyNode != null && valueNode != null)
                {
                    var key = keyNode.InnerText.Trim().TrimEnd(':');
                    var value = valueNode.InnerText.Trim();

                    dictionary[key] = value;
                }
            }
        }

        var lineDivs = contentNode.SelectNodes("//div[contains(@class,'line')]");
        if (lineDivs != null)
        {
            foreach (var div in lineDivs)
            {
                var keyNode = div.SelectSingleNode(".//div[contains(@class,'colLabel')]/label");
                var valueNode = div.SelectSingleNode(".//div[contains(@class,'colValue')]/span");

                if (keyNode != null && valueNode != null)
                {
                    var key = keyNode.InnerText.Trim().TrimEnd(':');
                    var value = valueNode.InnerText.Trim();

                    dictionary[key] = value;
                }
            }
        }

        return dictionary;
    }

    [Benchmark]
    public Dictionary<string, string> Version2()
    {
        var dictionary = new Dictionary<string, string>();

        var contentNode = _htmlDocument.DocumentNode.SelectSingleNode("//div[@class='h-tab-content-inner']");

        var keyValueDivs = contentNode.SelectNodes("//div[contains(@class,'keyvalue')]");
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

                    if (!true && (string.IsNullOrWhiteSpace(value) || value == "-"))
                        continue;

                    dictionary[key] = value;
                }
            }
        }

        var lineDivs = contentNode.SelectNodes("//div[contains(@class,'line') and @id!='lblHstrskVsnngLine']");
        if (lineDivs != null)
        {
            foreach (var div in lineDivs)
            {
                var keyNode = div.SelectSingleNode("./div[contains(@class,'colLabel')]");
                var valueNode = div.SelectSingleNode("./div[contains(@class,'colValue')]/span");

                if (keyNode != null && valueNode != null)
                {
                    var key = keyNode.InnerText.Trim().TrimEnd(':');
                    var value = valueNode.InnerText.Trim();

                    if (!true && (string.IsNullOrWhiteSpace(value) || value == "-"))
                        continue;

                    dictionary[key] = value;
                }
            }
        }

        return dictionary;
    }
}
