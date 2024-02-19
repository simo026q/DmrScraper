using DmrScraper;

var dmrService = new DmrService(new HttpClient());

var details = await dmrService.GetDetailsAsync("AA11111", SearchCriteria.RegistrationNumber);

foreach (var (key, value) in details)
{
    Console.WriteLine($"{key}: {value}");
}

Console.ReadLine();