using DmrScraper.Internal;
using HtmlAgilityPack;

namespace DmrScraper;

internal partial class DmrService
{
    private readonly HttpClient _client;

    public DmrService(HttpClient client)
    {
        _client = client;
    }

    public async Task<Dictionary<string, string>> GetDetailsAsync(string searchString, SearchCriteria searchCriteria)
    {
        var searchInfo = await GetSearchInfoAsync(searchString, searchCriteria);

        return await GetDetailsFromSearchInfoAsync(searchInfo);
    }

    private async Task<Dictionary<string, string>> GetDetailsFromSearchInfoAsync(SearchInfo searchInfo)
    {
        var content = searchInfo.GetFormUrlEncodedContent();
        var execution = searchInfo.Execution;

        var dictionary = new Dictionary<string, string>();

        var searchRequest = new HttpRequestMessage(HttpMethod.Post, DmrUriBuilder.CreateSearch(execution));
        searchRequest.Content = content;
        HttpResponseMessage searchResultResponse = await _client.SendAsync(searchRequest);

        string searchResult = await searchResultResponse.Content.ReadAsStringAsync();

        FillDictionaryFromHtml(dictionary, searchResult);

        execution.IncrementActionId();

        for (var i = 1; i <= 4; i++)
        {
            var pageRequest = new HttpRequestMessage(HttpMethod.Get, DmrUriBuilder.CreatePage(execution, i));
            HttpResponseMessage pageResponse = await _client.SendAsync(pageRequest);

            string pageHtml = await pageResponse.Content.ReadAsStringAsync();

            FillDictionaryFromHtml(dictionary, pageHtml);

            execution.IncrementActionId();
        }

        return dictionary;
    }

    private async Task<SearchInfo> GetSearchInfoAsync(string searchString, SearchCriteria searchCriteria)
    {
        HttpResponseMessage searchPageResponse = await _client.GetAsync(DmrUriBuilder.BaseUri);

        HtmlDocument htmlDocument = await searchPageResponse.Content.ReadAsHtmlDocumentAsync();

        HtmlNode formNode = htmlDocument.DocumentNode.SelectSingleNode("//form[@id='searchForm']");
        if (formNode == null) 
            throw new InvalidOperationException("Search form not found");

        HtmlNode formTokenNode = formNode.SelectSingleNode("//input[@name='dmrFormToken']");
        if (formTokenNode == null) 
            throw new InvalidOperationException("Form token not found");

        string formAction = formNode.GetAttributeValue("action", string.Empty);
        string formToken = formTokenNode.GetAttributeValue("value", string.Empty);

        return new SearchInfo(formToken, searchCriteria, searchString, DmrExecution.FromUri(formAction));
    }
        
    private static void FillDictionaryFromHtml(Dictionary<string, string> dictionary, string html)
    {
        var htmlDocument = new HtmlDocument();
        htmlDocument.LoadHtml(html);

        var contentNode = htmlDocument.DocumentNode.SelectSingleNode("//div[@class='h-tab-content-inner']");

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
    }
}
