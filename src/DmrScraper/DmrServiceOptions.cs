namespace DmrScraper;

/// <summary>
/// Represents options that can be applied when interacting with the Danish Motor Registry service.
/// </summary>
public sealed class DmrServiceOptions
{
    /// <summary>
    /// Default instance of <see cref="DmrServiceOptions"/> with default property values.
    /// </summary>
    public static readonly DmrServiceOptions Default = new();

    /// <summary>
    /// Gets or sets a value indicating whether to include empty values in the result. (A empty value is shown as "-" on the website)
    /// </summary>
    public bool IncludeEmptyValues { get; init; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether to include false values in the result. (A empty value is shown as "Nej" on the website)
    /// </summary>
    public bool IncludeFalseValues { get; init; } = true;
}
