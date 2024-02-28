using DmrScraper;

IDmrService dmrService = new DmrService(new HttpClient());

string registrationNumber = GeetRegistrationNumber();

DetailsResult? details = await dmrService.GetDetailsAsync(registrationNumber, SearchCriteria.RegistrationNumber, AdditionalSearchSheets.TechnicalInformation, new DmrServiceOptions() { IncludeEmptyValues = true });

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