namespace DmrScraper;

public interface IDmrService
{
    Task<DetailsResult?> GetDetailsAsync(string searchString, SearchCriteria searchCriteria, AdditionalSearchSheets searchSheets, DmrServiceOptions options);
}