namespace DmrScraper;

public sealed class DmrServiceOptions
{
    public bool IncludeEmptyValues { get; init; } = false;
    public bool IncludeFalseValues { get; init; } = true;
}
