namespace DmrScraper;

/// <summary>
/// Interface for interacting with the Danish Motor Registry service to retrieve details about vehicles.
/// </summary>
public interface IDmrService
{
    /// <summary>
    /// Retrieves details about a vehicle from the Danish Motor Registry.
    /// </summary>
    /// <param name="searchString">The value to be searched for.</param>
    /// <param name="searchCriteria">Specifies the criteria for the search operation.</param>
    /// <param name="searchSheets">Determines which additional data sheets are to be included in the search.</param>
    /// <param name="options">Optional settings for customizing the service behavior.</param>
    /// <returns>
    /// A <see cref="DetailsResult"/> instance containing information about the vehicle, 
    /// or <see langword="null"/> if the vehicle is not found.
    /// </returns>
    Task<DetailsResult?> GetDetailsAsync(string searchString, SearchCriteria searchCriteria, AdditionalSearchSheets searchSheets, DmrServiceOptions? options = null);

    /// <summary>
    /// Retrieves details about a vehicle from the Danish Motor Registry.
    /// </summary>
    /// <param name="searchString">The value to be searched for.</param>
    /// <param name="searchCriteria">Specifies the criteria for the search operation.</param>
    /// <param name="options">Optional settings for customizing the service behavior.</param>
    /// <returns>
    /// A <see cref="DetailsResult"/> instance containing information about the vehicle, 
    /// or <see langword="null"/> if the vehicle is not found.
    /// </returns>
    Task<DetailsResult?> GetDetailsAsync(string searchString, SearchCriteria searchCriteria, DmrServiceOptions? options = null);
}