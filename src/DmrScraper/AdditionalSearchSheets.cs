namespace DmrScraper;

/// <summary>
/// Specifies additional data sheets to include in the search when querying the Danish Motor Registry service.
/// </summary>
[Flags]
public enum AdditionalSearchSheets
{
    /// <summary>
    /// No additional sheets included.
    /// </summary>
    None = 0,

    /// <summary>
    /// Include technical information sheet.
    /// </summary>
    TechnicalInformation = 1,

    Inspection = 2,
    Insurance = 4,
    //Permits = 8,

    /// <summary>
    /// Include all available sheets.
    /// </summary>
    All = TechnicalInformation | Inspection | Insurance //| Permits
}