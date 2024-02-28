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
        var searchInfo = await GetSearchInfoAsync(searchString, searchCriteria);

        return await GetDetailsFromSearchInfoAsync(searchInfo, searchSheets, options ?? DmrServiceOptions.Default);
    }

    /// <inheritdoc cref="IDmrService"/>
    public Task<DetailsResult?> GetDetailsAsync(string searchString, SearchCriteria searchCriteria, DmrServiceOptions? options = null)
    {
        return GetDetailsAsync(searchString, searchCriteria, AdditionalSearchSheets.None, options);
    }

    private async Task<DetailsResult?> GetDetailsFromSearchInfoAsync(SearchInfo searchInfo, AdditionalSearchSheets searchSheets, DmrServiceOptions options)
    {
        var content = searchInfo.GetFormUrlEncodedContent();
        var execution = searchInfo.Execution;

        HttpResponseMessage searchResultResponse = await _client.PostAsync(DmrUriBuilder.CreateSearch(execution), content);

        HtmlDocument searchResult = await searchResultResponse.Content.ReadAsHtmlDocumentAsync();

        var reader = new DmrHtmlReader(searchResult);

        if (!reader.HasContent)
            return null;

        var vehicleDetails = reader.ReadKeyValuePairs(options.IncludeEmptyValues, options.IncludeFalseValues);

        execution.IncrementActionId();

        IEnumerable<KeyValuePair<string, string>> technicalInformation = searchSheets.HasFlag(AdditionalSearchSheets.TechnicalInformation)
            ? await GetDetailsFromPageIndexAsync(1, execution, options)
            : [];

        IEnumerable<KeyValuePair<string, string>> inspectionDetails = searchSheets.HasFlag(AdditionalSearchSheets.Inspection)
            ? await GetDetailsFromPageIndexAsync(2, execution, options)
            : [];

        IEnumerable<KeyValuePair<string, string>> insuranceDetails = searchSheets.HasFlag(AdditionalSearchSheets.Insurance)
            ? await GetDetailsFromPageIndexAsync(3, execution, options)
            : [];

        return new DetailsResult(vehicleDetails, technicalInformation, inspectionDetails, insuranceDetails);
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

    private async Task<IEnumerable<KeyValuePair<string, string>>> GetDetailsFromPageIndexAsync(int pageIndex, DmrExecution execution, DmrServiceOptions options)
    {
        HttpResponseMessage pageResponse = await _client.GetAsync(DmrUriBuilder.CreatePage(execution, pageIndex));

        HtmlDocument pageHtml = await pageResponse.Content.ReadAsHtmlDocumentAsync();

        execution.IncrementActionId();

        var reader = new DmrHtmlReader(pageHtml);

        return reader.ReadKeyValuePairs(options.IncludeEmptyValues, options.IncludeFalseValues);
    }
}