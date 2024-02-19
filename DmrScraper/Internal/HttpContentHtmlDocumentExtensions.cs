using HtmlAgilityPack;

namespace DmrScraper.Internal;

internal static class HttpContentHtmlDocumentExtensions
{
    private const string AcceptableHtmlMediaType = "text/html";

    public static async Task<HtmlDocument> ReadAsHtmlDocumentAsync(this HttpContent httpContent)
    {
        if (httpContent.Headers.ContentType != null && httpContent.Headers.ContentType.MediaType != AcceptableHtmlMediaType)
        {
            throw new InvalidOperationException("Content type is not text/html");
        }

        var html = await httpContent.ReadAsStringAsync();

        var htmlDocument = new HtmlDocument();
        htmlDocument.LoadHtml(html);

        return htmlDocument;
    }
}
