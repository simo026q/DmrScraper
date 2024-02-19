// See https://aka.ms/new-console-template for more information

using DmrScraper;

var dmrService = new DmrService(new HttpClient());

var details = await dmrService.GetDetailsAsync("DP74766", SearchCriteria.RegistrationNumber);

foreach (var (key, value) in details)
{
    Console.WriteLine($"{key}: {value}");
}

Console.ReadLine();