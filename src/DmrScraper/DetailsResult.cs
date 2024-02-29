namespace DmrScraper;

/// <summary>
/// Represents the result of a query for details about a vehicle in the Danish Motor Registry service.
/// </summary>
public sealed class DetailsResult
{
    /// <summary>
    /// Gets the details of the vehicle.
    /// </summary>
    public IEnumerable<KeyValuePair<string, string>> Vehicle { get; }

    /// <summary>
    /// Gets the technical information about the vehicle.
    /// </summary>
    public IEnumerable<KeyValuePair<string, string>> TechnicalInformation { get; }

    /// <summary>
    /// Gets the inspection details of the vehicle.
    /// </summary>
    public IEnumerable<KeyValuePair<string, string>> Inspection { get; }

    /// <summary>
    /// Gets the insurance details of the vehicle.
    /// </summary>
    public IEnumerable<KeyValuePair<string, string>> Insurance { get; }

    //public IEnumerable<KeyValuePair<string, string>> Permits { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="DetailsResult"/> class with the specified vehicle details.
    /// </summary>
    /// <param name="vehicle">The details of the vehicle.</param>
    /// <param name="technicalInformation">The technical information about the vehicle.</param>
    /// <param name="inspection">The inspection details of the vehicle.</param>
    /// <param name="insurance">The insurance details of the vehicle.</param>s
    internal DetailsResult(IEnumerable<KeyValuePair<string, string>> vehicle, 
        IEnumerable<KeyValuePair<string, string>> technicalInformation,
        IEnumerable<KeyValuePair<string, string>> inspection,
        IEnumerable<KeyValuePair<string, string>> insurance)
    {
        Vehicle = vehicle;
        TechnicalInformation = technicalInformation;
        Inspection = inspection;
        Insurance = insurance;
    }

    /// <summary>
    /// Gets all combined details of the vehicle including vehicle details, technical information, inspection, and insurance.
    /// </summary>
    /// <returns>All combined details of the vehicle.</returns>
    public IEnumerable<KeyValuePair<string, string>> GetAllCombined()
    {
        return Vehicle.Concat(TechnicalInformation).Concat(Inspection).Concat(Insurance);
    }
}