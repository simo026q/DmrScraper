using DmrScraper.Internal;
using HtmlAgilityPack;

namespace DmrScraper;

/// <inheritdoc cref="IDmrService"/>
public class DmrService(HttpClient client) 
    : IDmrService
{
    private readonly HttpClient _client = client;

    /// <inheritdoc cref="IDmrService"/>
    public async Task<DetailsResult?> GetDetailsAsync(string searchString, SearchCriteria searchCriteria, AdditionalSearchSheets searchSheets, DmrServiceOptions? options = null)
    {
        SearchInfo? searchInfo = await GetSearchInfoAsync(searchString, searchCriteria);
        if (searchInfo == null)
            return null;

        return await GetDetailsFromSearchInfoAsync(searchInfo, searchSheets, options ?? DmrServiceOptions.Default);
    }

    /// <inheritdoc cref="IDmrService"/>
    public Task<DetailsResult?> GetDetailsAsync(string searchString, SearchCriteria searchCriteria, DmrServiceOptions? options = null)
    {
        return GetDetailsAsync(searchString, searchCriteria, AdditionalSearchSheets.None, options);
    }

    private async Task<DetailsResult?> GetDetailsFromSearchInfoAsync(SearchInfo searchInfo, AdditionalSearchSheets searchSheets, DmrServiceOptions options)
    {
        FormUrlEncodedContent content = searchInfo.GetFormUrlEncodedContent();

        HttpResponseMessage searchResultResponse;
        try
        {
            searchResultResponse = await _client.PostAsync(DmrUriBuilder.CreateSearch(searchInfo.Execution), content);
            if (!searchResultResponse.IsSuccessStatusCode)
                return null;
        }
        catch (HttpRequestException)
        {
            return null;
        }
        
        HtmlDocument searchResult = await searchResultResponse.Content.ReadAsHtmlDocumentAsync();

        var reader = new DmrHtmlReader(searchResult, DmrHtmlReader.ReadStrategy.InsideFieldGroup);

        if (!reader.HasContent)
            return null;

        IEnumerable<KeyValuePair<string, string>> vehicleDetails = reader.ReadKeyValuePairs(options.IncludeEmptyValues, options.IncludeFalseValues);

        Task<IEnumerable<KeyValuePair<string, string>>> technicalTask = searchSheets.HasFlag(AdditionalSearchSheets.TechnicalInformation) 
            ? GetDetailsFromPageIndexAsync(1, searchInfo.Execution, DmrHtmlReader.ReadStrategy.InsideFieldGroup, options) 
            : Task.FromResult(Enumerable.Empty<KeyValuePair<string, string>>());

        Task<IEnumerable<KeyValuePair<string, string>>> inspectionTask = searchSheets.HasFlag(AdditionalSearchSheets.Inspection) 
            ? GetDetailsFromPageIndexAsync(2, searchInfo.Execution, DmrHtmlReader.ReadStrategy.InsideContent, options) 
            : Task.FromResult(Enumerable.Empty<KeyValuePair<string, string>>());

        Task<IEnumerable<KeyValuePair<string, string>>> insuranceTask = searchSheets.HasFlag(AdditionalSearchSheets.Insurance) 
            ? GetDetailsFromPageIndexAsync(3, searchInfo.Execution, DmrHtmlReader.ReadStrategy.InsideContent, options) 
            : Task.FromResult(Enumerable.Empty<KeyValuePair<string, string>>());

        await Task.WhenAll(technicalTask, inspectionTask, insuranceTask);

        return new DetailsResult(vehicleDetails, technicalTask.Result, inspectionTask.Result, insuranceTask.Result);
    }

    private async Task<SearchInfo?> GetSearchInfoAsync(string searchString, SearchCriteria searchCriteria)
    {
        HttpResponseMessage searchPageResponse;
        try
        {
            searchPageResponse = await _client.GetAsync(DmrUriBuilder.BaseUri);
            if (!searchPageResponse.IsSuccessStatusCode)
                return null;
        }
        catch (HttpRequestException)
        {
            return null;
        }

        HtmlDocument htmlDocument = await searchPageResponse.Content.ReadAsHtmlDocumentAsync();

        HtmlNode formNode = htmlDocument.DocumentNode.SelectSingleNode("//form[@id='searchForm']");
        if (formNode == null)
            return null;

        HtmlNode formTokenNode = formNode.SelectSingleNode("//input[@name='dmrFormToken']");
        if (formTokenNode == null)
            return null;

        string formAction = formNode.GetAttributeValue("action", string.Empty);
        string formToken = formTokenNode.GetAttributeValue("value", string.Empty);

        return new SearchInfo(formToken, searchCriteria, searchString, DmrExecution.FromUri(formAction));
    }

    private async Task<IEnumerable<KeyValuePair<string, string>>> GetDetailsFromPageIndexAsync(int pageIndex, DmrExecution execution, DmrHtmlReader.ReadStrategy readStrategy, DmrServiceOptions options)
    {
        HttpResponseMessage pageResponse;
        try
        {
            pageResponse = await _client.GetAsync(DmrUriBuilder.CreatePage(execution, pageIndex));
            if (!pageResponse.IsSuccessStatusCode)
                return [];
        }
        catch (HttpRequestException)
        {
            return [];
        }

        HtmlDocument pageHtml = await pageResponse.Content.ReadAsHtmlDocumentAsync();

        var reader = new DmrHtmlReader(pageHtml, readStrategy);

        return reader.ReadKeyValuePairs(options.IncludeEmptyValues, options.IncludeFalseValues);
    }
}