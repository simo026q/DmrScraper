// See https://aka.ms/new-console-template for more information
using BenchmarkDotNet.Attributes;
using HtmlAgilityPack;

[MemoryDiagnoser(false)]
public class HtmlLoadBenchmarks
{
    private static readonly HttpClient _httpClient = new();

    private HttpResponseMessage _htmlResponse;

    [GlobalSetup]
    public async Task Setup()
    {
        _htmlResponse = await _httpClient.GetAsync("https://www.google.com");
    }

    [Benchmark]
    public async Task<HtmlDocument> LoadStream()
    {
        var htmlStream = await _htmlResponse.Content.ReadAsStreamAsync();

        var htmlDocument = new HtmlDocument();
        htmlDocument.Load(htmlStream);

        return htmlDocument;
    }

    [Benchmark]
    public async Task<HtmlDocument> LoadString()
    {
        var htmlString = await _htmlResponse.Content.ReadAsStringAsync();

        var htmlDocument = new HtmlDocument();
        htmlDocument.LoadHtml(htmlString);

        return htmlDocument;
    }
}