namespace DmrScraper;

/// <summary>
/// Specifies the criteria for searching vehicle details in the Danish Motor Registry service.
/// </summary>
public enum SearchCriteria
{
    /// <summary>
    /// Search by registration number.
    /// </summary>
    RegistrationNumber,

    /// <summary>
    /// Search by vehicle identification number (VIN).
    /// </summary>
    Vin,

    /// <summary>
    /// Search by vehicle ID.
    /// </summary>
    VehicleId
}
