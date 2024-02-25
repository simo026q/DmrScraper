using DmrScraper.Internal;
using DmrScraper.Models;
using HtmlAgilityPack;

namespace DmrScraper;

public class DmrService(HttpClient client) 
    : IDmrService
{
    private readonly HttpClient _client = client;

    public async Task<List<KeyValuePair<string, string>>> GetDetailsAsync(string searchString, SearchCriteria searchCriteria, bool includeEmpty = false)
    {
        var searchInfo = await GetSearchInfoAsync(searchString, searchCriteria);

        return await GetDetailsFromSearchInfoAsync(searchInfo, includeEmpty, includeFalse: true);
    }

    public async Task<Vehicle> GetVehicleAsync(string searchString, SearchCriteria searchCriteria)
    {
        var searchInfo = await GetSearchInfoAsync(searchString, searchCriteria);

        var details = await GetDetailsFromSearchInfoAsync(searchInfo, includeEmpty: false, includeFalse: false);

        return VehicleParser.ParseVehicle(details);
    }

    private async Task<List<KeyValuePair<string, string>>> GetDetailsFromSearchInfoAsync(SearchInfo searchInfo, bool includeEmpty, bool includeFalse)
    {
        var content = searchInfo.GetFormUrlEncodedContent();
        var execution = searchInfo.Execution;

        var list = new List<KeyValuePair<string, string>>();

        HttpResponseMessage searchResultResponse = await _client.PostAsync(DmrUriBuilder.CreateSearch(execution), content);

        HtmlDocument searchResult = await searchResultResponse.Content.ReadAsHtmlDocumentAsync();

        FillListFromHtml(list, searchResult, includeEmpty, includeFalse);

        execution.IncrementActionId();

        for (var i = 1; i <= 4; i++)
        {
            HttpResponseMessage pageResponse = await _client.GetAsync(DmrUriBuilder.CreatePage(execution, i));

            HtmlDocument pageHtml = await pageResponse.Content.ReadAsHtmlDocumentAsync();

            FillListFromHtml(list, pageHtml, includeEmpty, includeFalse);

            execution.IncrementActionId();
        }

        return list;
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
        
    private static void FillListFromHtml(List<KeyValuePair<string, string>> list, HtmlDocument htmlDocument, bool includeEmpty, bool includeFalse)
    {
        var reader = new DmrHtmlReader(htmlDocument);

        list.AddRange(reader.ReadKeyValuePairs(includeEmpty, includeFalse));
    }
}