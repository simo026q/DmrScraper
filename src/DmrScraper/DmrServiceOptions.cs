namespace DmrScraper;

public sealed class DmrServiceOptions
{
    public static readonly DmrServiceOptions Default = new();

    public bool IncludeEmptyValues { get; init; } = false;
    public bool IncludeFalseValues { get; init; } = true;
}
