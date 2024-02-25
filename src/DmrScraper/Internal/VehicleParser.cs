using DmrScraper.Models;

namespace DmrScraper.Internal;

internal static class VehicleParser
{
    public static Vehicle ParseVehicle(List<KeyValuePair<string, string>> details)
    {
        var vehicle = new Vehicle();

        foreach (var (key, value) in details)
        {
            switch (key)
            {
                case "KøretøjsID":
                    vehicle.Id = value;
                    break;
                case "Stelnummer":
                    vehicle.Vin = value;
                    break;
                case "Mærke, Model, Variant":
                    var parts = value.Split(", ", 3);
                    vehicle.Make = parts[0];
                    vehicle.Model = parts[1];
                    vehicle.Variant = parts[2];
                    break;
                case "Art":
                    vehicle.VehicleType = value;
                    break;
                case "Seneste ændring":
                    if (value.Contains("Registreret"))
                    {
                        var dateStr = value.Replace("Registreret d. ", "");
                        vehicle.RegistrationLastUpdatedAt = DateOnly.ParseExact(dateStr, "dd-MM-yyyy", null);
                    }
                    else if (value.Contains("Afmeldt"))
                    {
                        // TODO: Handle afmeldt
                    }
                    else
                    {
                        var dateStr = value.Replace("d. ", "");
                        vehicle.LastUpdatedAt = DateOnly.ParseExact(dateStr, "dd-MM-yyyy", null);
                    }
                    break;
                case "Registreringsnummer":
                    vehicle.LicensePlate = value;
                    break;
                case "1. registreringsdato":
                    vehicle.FirstRegistratedAt = DateOnly.ParseExact(value, "dd-MM-yyyy", null);
                    break;
                case "Anvendelse":
                    vehicle.Usage = value;
                    break;
                case "EF-Type-godkendelsenr.":
                    vehicle.EfTypeApprovalNumber = value;
                    break;
                case "Typeanmeldelses­nummer/bremsedata-erklæringsnummer":
                    vehicle.TypeApprovalNumber = value;
                    break;
                case "Supplerende anvendelser":
                    vehicle.AddtionalUsages = value;
                    break;
                case "Status":
                    vehicle.Status = value;
                    break;
                case "Type":
                    vehicle.Type = value;
                    break;
                case "EU-variant":
                    vehicle.EuVariant = value;
                    break;
                case "EU-version":
                    vehicle.EuVersion = value;
                    break;
                case "Kategori":
                    vehicle.Category = value;
                    break;
                case "Fabrikant":
                    vehicle.Manufacturer = value;
                    break;
                case "Farve":
                    vehicle.Color = value;
                    break;
                case "Model-år":
                    if (int.TryParse(value, out var modelYear))
                    {
                        vehicle.ModelYear = modelYear;
                    }
                    break;
                case "Ibrugtagningsdato":
                    //vehicle.CommissionedAt = DateOnly.ParseExact(value, "dd-MM-yyyy", null);
                    break;
                case "Bestået NCAP test med mindst 5 stjerner":
                    vehicle.FiveStarsNcapRating = value == "Ja";
                    break;
                case "Fuelmode":
                    vehicle.FuelMode = value;
                    break;
                case "Kilometerstand (.000 km)":
                    if (int.TryParse(value, out var mileage))
                    {
                        vehicle.Mileage = mileage * 1000;
                    }
                    break;
                case "Dokumentation for kilometerstand":
                    vehicle.MileageDocumentation = value;
                    break;
                case "Køretøj stand":
                    vehicle.Condition = value;
                    break;
                case "Bemærkninger for stand":
                    vehicle.ConditionRemarks = value;
                    break;
                case "Trafikskade":
                    vehicle.TrafficDamage = value;
                    break;
            }
        }

        return vehicle;
    }
}
