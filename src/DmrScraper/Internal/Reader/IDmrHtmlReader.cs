namespace DmrScraper.Internal.Reader;

internal interface IDmrHtmlReader 
    : IReadOnlyDictionary<string, string>
{
    bool HasContent { get; }
    DmrPage Page { get; }
}
