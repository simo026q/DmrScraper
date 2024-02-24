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
                case "Stelnummer":
                    vehicle.Vin = value;
                    break;
                case "Mærke, Model, Variant":
                    var parts = value.Split(", ", 3);
                    vehicle.Make = parts[0];
                    vehicle.Model = parts[1];
                    vehicle.Variant = parts[2];
                    break;
            }
        }

        return vehicle;
    }
}
