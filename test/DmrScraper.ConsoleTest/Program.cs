﻿using DmrScraper;
using System.Text.Json;

var dmrService = new DmrService(new HttpClient());

string registrationNumber = GeetRegistrationNumber();

var vehicle = await dmrService.GetVehicleAsync(registrationNumber, SearchCriteria.RegistrationNumber);

var vehicleJson = JsonSerializer.Serialize(vehicle, new JsonSerializerOptions { WriteIndented = true });

Console.WriteLine(vehicleJson);

/*var details = await dmrService.GetDetailsAsync(registrationNumber, SearchCriteria.RegistrationNumber, includeEmpty: true);

foreach (var (key, value) in details)
{
    Console.WriteLine($"{key}: {value}");
}*/

Console.WriteLine("Press any key to exit...");
Console.ReadKey();

static string GeetRegistrationNumber()
{
    string? registrationNumber;

    do
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.Write("? ");
        Console.ForegroundColor = ConsoleColor.White;
        Console.Write("Enter registration number: ");
        Console.ForegroundColor = ConsoleColor.Gray;

        registrationNumber = Console.ReadLine();

        Console.ResetColor();
    }
    while (string.IsNullOrWhiteSpace(registrationNumber));

    return registrationNumber;
}