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

                // Equipment
                case "1- eller 2-zone klima":
                case "3- eller 4-zone klima":
                    if (value == "Ja")
                    {
                        vehicle.Equipment.Add(new VehicleEquipment("climate_control", 1));
                    }
                    break;
                case "Afstandsradar":
                    if (value == "Ja")
                    {
                        vehicle.Equipment.Add(new VehicleEquipment("distance_radar", 1));
                    }
                    break;
                case "Aktiv fartpilot":
                    if (value == "Ja")
                    {
                        vehicle.Equipment.Add(new VehicleEquipment("adaptive_cruise_control", 1));
                    }
                    break;
                case "Antal selealarmer":
                    if (int.TryParse(value, out var seatBeltAlarmCount) && seatBeltAlarmCount > 0)
                    {
                        vehicle.Equipment.Add(new VehicleEquipment("seat_belt_alarms", seatBeltAlarmCount));
                    }
                    break;
                case "Bakkamera":
                    if (value == "Ja")
                    {
                        vehicle.Equipment.Add(new VehicleEquipment("rear_view_camera", 1));
                    }
                    break;
                case "El-opvarmet forrude":
                    if (value == "Ja")
                    {
                        vehicle.Equipment.Add(new VehicleEquipment("heated_windshield", 1));
                    }
                    break;
                case "Eelektrisk bagklap":
                    if (value == "Ja")
                    {
                        vehicle.Equipment.Add(new VehicleEquipment("electric_tailgate", 1));
                    }
                    break;
                case "Elektrisk lukning af døre":
                    if (value == "Ja")
                    {
                        vehicle.Equipment.Add(new VehicleEquipment("electricly_closing_doors", 1));
                    }
                    break;
                case "Head-up display":
                    if (value == "Ja")
                    {
                        vehicle.Equipment.Add(new VehicleEquipment("head_up_display", 1));
                    }
                    break;
                case "HiFi musikanlæg":
                    if (value == "Ja")
                    {
                        vehicle.Equipment.Add(new VehicleEquipment("hifi_sound_system", 1));
                    }
                    break;
                case "Key-less go (nøglefri)":
                    if (value == "Ja")
                    {
                        vehicle.Equipment.Add(new VehicleEquipment("keyless_go", 1));
                    }
                    break;
                case "Linievogter":
                    if (value == "Ja")
                    {
                        vehicle.Equipment.Add(new VehicleEquipment("lane_keeping_assist", 1));
                    }
                    break;
                case "Manuel Aircondition":
                    if (value == "Ja")
                    {
                        vehicle.Equipment.Add(new VehicleEquipment("manual_air_conditioning", 1));
                    }
                    break;
                case "Natsyns-udstyr":
                    if (value == "Ja")
                    {
                        vehicle.Equipment.Add(new VehicleEquipment("night_vision", 1));
                    }
                    break;
                case "Navigationssystem med skærm":
                    if (value == "Ja")
                    {
                        vehicle.Equipment.Add(new VehicleEquipment("navigation_system", 1));
                    }
                    break;
                case "Original tyverialarm":
                    if (value == "Ja")
                    {
                        vehicle.Equipment.Add(new VehicleEquipment("original_theft_alarm", 1));
                    }
                    break;
                case "Parkeringsassistent":
                    if (value == "Ja")
                    {
                        vehicle.Equipment.Add(new VehicleEquipment("parking_assist", 1));
                    }
                    break;
                case "Parkeringskontrol for":
                    if (value == "Ja")
                    {
                        vehicle.Equipment.Add(new VehicleEquipment("parking_sensors_front", 1));
                    }
                    break;
                case "Parkeringskontrol bag":
                    if (value == "Ja")
                    {
                        vehicle.Equipment.Add(new VehicleEquipment("parking_sensors_rear", 1));
                    }
                    break;
                /*case "Solcellekøling, kabine":
                    if (value == "Ja")
                    {
                        vehicle.Equipment.Add(new VehicleEquipment("solar_cell_cooling_cabin", 1));
                    }
                    break;*/
                case "Stemmestyring":
                    if (value == "Ja")
                    {
                        vehicle.Equipment.Add(new VehicleEquipment("voice_control", 1));
                    }
                    break;
                case "Vognbaneskift-alarm":
                    if (value == "Ja")
                    {
                        vehicle.Equipment.Add(new VehicleEquipment("lane_departure_warning", 1));
                    }
                    break;
                /*case "3 el. flere sæderækker":
                    break;*/
                /*case "Dobbeltkabine":
                    break;*/
                case "El-soltag":
                    if (value == "Ja")
                    {
                        vehicle.Equipment.Add(new VehicleEquipment("electric_sunroof", 1));
                    }
                    break;
                case "Glastag":
                    if (value == "Ja")
                    {
                        vehicle.Equipment.Add(new VehicleEquipment("sunroof", 1));
                    }
                    break;
                case "Kurvelys":
                    if (value == "Ja")
                    {
                        vehicle.Equipment.Add(new VehicleEquipment("cornering_lights", 1));
                    }
                    break;
                case "Metallak":
                    if (value == "Ja")
                    {
                        vehicle.Equipment.Add(new VehicleEquipment("matallic_paint", 1));
                    }
                    break;
                case "Targa":
                    if (value == "Ja")
                    {
                        vehicle.Equipment.Add(new VehicleEquipment("targa_roof", 1));
                    }
                    break;
                /*case "Uden siderude i (varerum) bag førersædet i bilens venstre side":
                    break;*/
                case "Xenon forlygter":
                    if (value == "Ja")
                    {
                        vehicle.Equipment.Add(new VehicleEquipment("xenon_headlights", 1));
                    }
                    break;
                /*case "6-gear manuel":
                    break;*/
                case "ESC stabilitetskontrol":
                    if (value == "Ja")
                    {
                        vehicle.Equipment.Add(new VehicleEquipment("electronic_stability_control", 1));
                    }
                    break;
                /*case "Kompressor":
                    break;*/
                /*case "Motor/kabinevarmer":
                    break;*/
                /*case "Motornummer":
                    break;*/
                /*case "Tunet/anden motor":
                    break;*/
                case "ABS bremser":
                    if (value == "Ja")
                    {
                        vehicle.Equipment.Add(new VehicleEquipment("anti_lock_braking", 1));
                    }
                    break;
                case "Keramiske skiver":
                    if (value == "Ja")
                    {
                        vehicle.Equipment.Add(new VehicleEquipment("ceramic_brakes", 1));
                    }
                    break;
                case "Skivebremser for":
                    if (value == "Ja")
                    {
                        vehicle.Equipment.Add(new VehicleEquipment("disk_brakes_front", 1));
                    }
                    break;
                case "Skivebremser bag":
                    if (value == "Ja")
                    {
                        vehicle.Equipment.Add(new VehicleEquipment("disk_brakes_rear", 1));
                    }
                    break;
                /*case "Affjedret stel":
                    break;*/
                case "Elektroniske dæmpere":
                    if (value == "Ja")
                    {
                        vehicle.Equipment.Add(new VehicleEquipment("electronic_dampers", 1));
                    }
                    break;
                case "Luftaffjedring":
                    if (value == "Ja")
                    {
                        vehicle.Equipment.Add(new VehicleEquipment("air_suspension", 1));
                    }
                    break;
                case "Niveauregulering":
                    if (value == "Ja")
                    {
                        vehicle.Equipment.Add(new VehicleEquipment("leveling", 1));
                    }
                    break;
                /*case "Ombygget stel":
                    break;*/
                /*case "Stift stel":
                    break;*/
                /*case "Større hjul end 20\"":
                    break;*/
                case "Antal airbags":
                    if (int.TryParse(value, out var airbagCount) && airbagCount > 0)
                    {
                        vehicle.Equipment.Add(new VehicleEquipment("airbags", airbagCount));
                    }
                    break;
                case "Radio":
                    if (value == "Ja")
                    {
                        vehicle.Equipment.Add(new VehicleEquipment("radio", 1));
                    }
                    break;
                /*case "Automatgear":
                    break; */
                /*case "Firehjulstræk (4WD)":
                    break;*/
                case "Ratbetjent gear":
                    if (value == "Ja")
                    {
                        vehicle.Equipment.Add(new VehicleEquipment("steering_wheel_gear", 1));
                    }
                    break;
                case "Del-lædersæder":
                    if (value == "Ja")
                    {
                        vehicle.Equipment.Add(new VehicleEquipment("leather_seats_partly", 1));
                    }
                    break;
                case "El-gardiner i bagdøre":
                    if (value == "Ja")
                    {
                        vehicle.Equipment.Add(new VehicleEquipment("electric_curtains_rear_doors", 1));
                    }
                    break;
                case "El-gardiner i bagrude":
                    if (value == "Ja")
                    {
                        vehicle.Equipment.Add(new VehicleEquipment("electric_curtains_rear_window", 1));
                    }
                    break;
                case "El-indstillelige sæder bag":
                    if (value == "Ja")
                    {
                        vehicle.Equipment.Add(new VehicleEquipment("electric_adjustable_seats_rear", 1));
                    }
                    break;
                case "Faste sidetasker":
                    if (value == "Ja")
                    {
                        vehicle.Equipment.Add(new VehicleEquipment("fixed_side_bags", 1));
                    }
                    break;
                case "Integreret barnesæde":
                    if (int.TryParse(value, out var childSeatCount) && childSeatCount > 0)
                    {
                        vehicle.Equipment.Add(new VehicleEquipment("integrated_child_seats", childSeatCount));
                    }
                    break;
                case "Læder/skindsæder":
                    if (value == "Ja")
                    {
                        vehicle.Equipment.Add(new VehicleEquipment("leather_seats", 1));
                    }
                    break;
                case "Massagesæder":
                    if (value == "Ja")
                    {
                        vehicle.Equipment.Add(new VehicleEquipment("massage_seats", 1));
                    }
                    break;
                case "Memory-sæder for":
                    if (value == "Ja")
                    {
                        vehicle.Equipment.Add(new VehicleEquipment("memory_seats", 1));
                    }
                    break;
                case "Sport-/komfortsæder":
                    if (value == "Ja")
                    {
                        vehicle.Equipment.Add(new VehicleEquipment("sport_or_comfort_seats", 1));
                    }
                    break;
                case "Ventilation i sæder":
                    if (value == "Ja")
                    {
                        vehicle.Equipment.Add(new VehicleEquipment("ventilated_seats", 1));
                    }
                    break;
                case "El-indstilleligt rat":
                    if (value == "Ja")
                    {
                        vehicle.Equipment.Add(new VehicleEquipment("electric_adjustable_steering_wheel", 1));
                    }
                    break;
                /*case "Højrestyring":
                    break;*/
                /*case "Lang forgaffel":
                    break;*/
                case "Multifunktionsrat":
                    if (value == "Ja")
                    {
                        vehicle.Equipment.Add(new VehicleEquipment("multifunction_steering_wheel", 1));
                    }
                    break;
                case "Opvarmet rat/styr":
                    if (value == "Ja")
                    {
                        vehicle.Equipment.Add(new VehicleEquipment("heated_steering_wheel", 1));
                    }
                    break;
                /*case "Turbo":
                    break;*/
                case "Andet udstyr":
                    var equipment = value.Split(", ");
                    foreach (var eq in equipment)
                    {
                        vehicle.OtherEquipment.Add(eq);
                    }
                    break;
            }
        }

        return vehicle;
    }
}
