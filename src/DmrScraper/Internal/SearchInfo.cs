namespace DmrScraper.Internal;

internal readonly struct SearchInfo(string formToken, SearchCriteria searchCriteria, string searchString, DmrExecution execution)
{
    private const string FormDataToken = "dmrFormToken";
    private const string FormDataSearchString = "soegeord";
    private const string FormDataSearchCriteria = "soegekriterie";

    private const string SearchCriteriaRegistrationNumber = "REGISTRERINGSNUMMER";
    private const string SearchCriteriaVin = "STELNUMMER";
    private const string SearchCriteriaVehicleId = "KID";

    private static readonly KeyValuePair<string, string> SearchCriteriaRegistrationNumberPair = new(FormDataSearchCriteria, SearchCriteriaRegistrationNumber);
    private static readonly KeyValuePair<string, string> SearchCriteriaVinPair = new(FormDataSearchCriteria, SearchCriteriaVin);
    private static readonly KeyValuePair<string, string> SearchCriteriaVehicleIdPair = new(FormDataSearchCriteria, SearchCriteriaVehicleId);

    public string FormToken { get; } = formToken;
    public SearchCriteria SearchCriteria { get; } = searchCriteria;
    public string SearchString { get; } = searchString;
    public DmrExecution Execution { get; } = execution;

    public FormUrlEncodedContent GetFormUrlEncodedContent()
    {
        return new FormUrlEncodedContent([
            new(FormDataToken, FormToken),
            new(FormDataSearchString, SearchString),
            GetSearchCriteriaFormData(SearchCriteria)
        ]);
    }

    private static KeyValuePair<string, string> GetSearchCriteriaFormData(SearchCriteria searchCriteria)
    {
        return searchCriteria switch
        {
            SearchCriteria.RegistrationNumber => SearchCriteriaRegistrationNumberPair,
            SearchCriteria.Vin => SearchCriteriaVinPair,
            SearchCriteria.VehicleId => SearchCriteriaVehicleIdPair,
            _ => throw new ArgumentOutOfRangeException(nameof(searchCriteria), searchCriteria, null)
        };
    }
}
