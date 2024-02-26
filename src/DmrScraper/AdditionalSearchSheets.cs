namespace DmrScraper;

[Flags]
public enum AdditionalSearchSheets
{
    None = 0,
    TechnicalInformation = 1,
    //Inspection = 2,
    //Insurance = 4,
    //Permits = 8,
    All = TechnicalInformation //| Inspection | Insurance | Permits
}
