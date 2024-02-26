namespace DmrScraper;

public readonly struct DetailsResult(
    IEnumerable<KeyValuePair<string, string>> vehicle, 
    IEnumerable<KeyValuePair<string, string>> technicalInformation)
{ 
    public IEnumerable<KeyValuePair<string, string>> Vehicle { get; } = vehicle;
    public IEnumerable<KeyValuePair<string, string>> TechnicalInformation { get; } = technicalInformation;
    //public IEnumerable<KeyValuePair<string, string>> Inspection { get; }
    //public IEnumerable<KeyValuePair<string, string>> Insurance { get; }
    //public IEnumerable<KeyValuePair<string, string>> Permits { get; }
}
