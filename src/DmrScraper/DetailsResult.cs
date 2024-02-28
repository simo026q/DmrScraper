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

    public IEnumerable<KeyValuePair<string, string>> Inspection { get; }
    public IEnumerable<KeyValuePair<string, string>> Insurance { get; }

    //public IEnumerable<KeyValuePair<string, string>> Permits { get; }

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
}