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
}
