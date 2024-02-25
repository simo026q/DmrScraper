namespace DmrScraper.Models;

public class Vehicle
{
    public string Id { get; set; }
    public string Vin { get; set; }
    public string Make { get; set; }
    public string Model { get; set; }
    public string Variant { get; set; }
    public string VehicleType { get; set; }
    public DateOnly LastUpdatedAt { get; set; }
    public string LicensePlate { get; set; }
    public DateOnly FirstRegistratedAt { get; set; }
    public string Usage { get; set; }
    public DateOnly RegistrationLastUpdatedAt { get; set; }
    public string EfTypeApprovalNumber { get; set; }
    public string TypeApprovalNumber { get; set; }
    public string AddtionalUsages { get; set; }
    public string Status { get; set; }
    public string Type { get; set; }
    public string EuVariant { get; set; }
    public string EuVersion { get; set; }
    public string Category { get; set; }
    public string Manufacturer { get; set; }
    public string Color { get; set; }
    public int ModelYear { get; set; } = -1;
    public DateOnly CommissionedAt { get; set; }
    public bool FiveStarsNcapRating { get; set; } = false;
    public string FuelMode { get; set; }
    public int Mileage { get; set; }
    public string MileageDocumentation { get; set; }
    public string Condition { get; set; }
    public string ConditionRemarks { get; set; }
    public string TrafficDamage { get; set; }

    public IList<VehicleEquipment> Equipment { get; set; } = [];
    public ICollection<string> OtherEquipment { get; set; } = [];
}
