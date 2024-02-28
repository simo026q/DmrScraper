# DmrService

## Overview

The DmrService is a .NET library designed to facilitate the retrieval of detailed vehicle information based on various search criteria such as registration number, VIN, or vehicle ID. This service utilizes external web services to gather and parse vehicle data, offering a convenient interface for .NET applications.

## Features

- Retrieve vehicle details asynchronously.
- Search by registration number, VIN, or vehicle ID.
- Customize search behavior with `DmrServiceOptions`.
- Extendable to include additional search sheets.

## Getting Started

### Usage

1. Initialize the `DmrService` with an `HttpClient` instance.
2. Call `GetDetailsAsync` with the appropriate parameters to retrieve vehicle details.

### Example

```csharp
using DmrScraper;

IDmrService dmrService = new DmrService(new HttpClient());

DetailsResult? details = await dmrService.GetDetailsAsync(searchString: "AA11111", SearchCriteria.RegistrationNumber, AdditionalSearchSheets.TechnicalInformation, new DmrServiceOptions() { IncludeEmptyValues = true });

if (details == null)
{
    Console.WriteLine("No details found");
}
else
{
    Console.WriteLine();
    Console.WriteLine("Vehicle details:");
    Console.WriteLine();
    foreach (var (key, value) in details.Vehicle)
    {
        Console.WriteLine($"{key}: {value}");
    }

    Console.WriteLine();
    Console.WriteLine("Technical information:");
    Console.WriteLine();
    foreach (var (key, value) in details.TechnicalInformation)
    {
        Console.WriteLine($"{key}: {value}");
    }
}

Console.WriteLine("Press any key to exit...");
Console.ReadKey();

// ---- RESULT ----
//
// Vehicle details:
// 
// Stelnummer: VF7NC9HR8CY544509
// Mærke, Model, Variant: CITROËN, C4 VAN, EHDI 110
// Art: Varebil
// Seneste ændring: d. 28-11-2018
// Registreringsnummer: AA11111
// Første registrerings&shy;dato: 20-04-2012
// Anvendelse: Godstransport erhverv
// Seneste ændring: Afmeldt almindeligt d. 28-11-2018
// ... (additional fields)
//
// Technical information:
// 
// Vægt.Teknisk totalvægt: 1820
// Vægt.Totalvægt: 1820
// Vægt.Egenvægt: -
// Vægt.Køreklar vægt.Minimum: 1448
// Vægt.Køreklar vægt.Maksimum: -
// Vægt.V-værdi ved luftaffjedring: -
// Vægt.V-værdi ved mekanisk affjedring: -
// Vægt.Vogntogsvægt: -
// Vægt.Skammel belastning: -
// ... (additional fields)
```