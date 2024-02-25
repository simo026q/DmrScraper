namespace DmrScraper.Models;

public class VehicleEquipment(string name, int count)
{
    /// <summary>
    /// The name of the equipment in english as snake_case.
    /// </summary>
    public string Name { get; set; } = name;

    /// <summary>
    /// The count of the equipment.
    /// </summary>
    public int Count { get; set; } = count;

    internal static readonly IReadOnlyDictionary<string, string> NameDictionary = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        ["Bluetooth"] = "bluetooth",
        ["ISG"] = "integrated_starter_generator",
        ["Bakkamera"] = "rear_view_camera",
        ["Apple CarPlay"] = "apple_carplay",
        ["Android Auto"] = "android_auto",
        ["LED kørelys"] = "led_daytime_running_lights",
        ["Tågelygter"] = "fog_lights",
        ["Regnsensor"] = "rain_sensor",
        ["Sædevarme"] = "heated_seats"
    };
}
