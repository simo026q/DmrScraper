namespace DmrScraper.Internal.Reader;

internal interface IDmrHtmlReaderFactory
{
    IDmrHtmlReader Create(Stream htmlStream, DmrPage page);
}
