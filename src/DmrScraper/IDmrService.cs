using DmrScraper.Models;

namespace DmrScraper;

public interface IDmrService
{
    Task<List<KeyValuePair<string, string>>> GetDetailsAsync(string searchString, SearchCriteria searchCriteria, bool includeEmpty = false);
    Task<Vehicle> GetVehicleAsync(string searchString, SearchCriteria searchCriteria);
}