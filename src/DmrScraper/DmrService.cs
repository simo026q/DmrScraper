﻿using DmrScraper.Internal;
using HtmlAgilityPack;

namespace DmrScraper;

public class DmrService(HttpClient client) 
    : IDmrService
{
    private readonly HttpClient _client = client;

    public async Task<DetailsResult> GetDetailsAsync(string searchString, SearchCriteria searchCriteria, AdditionalSearchSheets searchSheets, DmrServiceOptions options)
    {
        var searchInfo = await GetSearchInfoAsync(searchString, searchCriteria);

        return await GetDetailsFromSearchInfoAsync(searchInfo, searchSheets, options);
    }

    private async Task<DetailsResult> GetDetailsFromSearchInfoAsync(SearchInfo searchInfo, AdditionalSearchSheets searchSheets, DmrServiceOptions options)
    {
        var content = searchInfo.GetFormUrlEncodedContent();
        var execution = searchInfo.Execution;

        HttpResponseMessage searchResultResponse = await _client.PostAsync(DmrUriBuilder.CreateSearch(execution), content);

        HtmlDocument searchResult = await searchResultResponse.Content.ReadAsHtmlDocumentAsync();

        var vehicleDetails = GetDetailsFromHtmlDocument(searchResult, options);

        execution.IncrementActionId();

        IEnumerable<KeyValuePair<string, string>> technicalInformation = searchSheets.HasFlag(AdditionalSearchSheets.TechnicalInformation)
            ? await GetDetailsFromPageIndexAsync(1, execution, options)
            : Enumerable.Empty<KeyValuePair<string, string>>();

        return new DetailsResult(vehicleDetails, technicalInformation);
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

        return GetDetailsFromHtmlDocument(pageHtml, options);
    }

    private static IEnumerable<KeyValuePair<string, string>> GetDetailsFromHtmlDocument(HtmlDocument htmlDocument, DmrServiceOptions options)
    {
        var reader = new DmrHtmlReader(htmlDocument);

        return reader.ReadKeyValuePairs(options.IncludeEmptyValues, options.IncludeFalseValues);
    }
}