// See https://aka.ms/new-console-template for more information
using BenchmarkDotNet.Attributes;
using DmrScraper;

Console.WriteLine("Hello, World!");

[MemoryDiagnoser(false)]
public class Benchmarks
{
    private readonly DmrService _dmrService = new(new HttpClient());

    [Benchmark]
    public async Task GetDetailsAsync()
    {
        await _dmrService.GetDetailsAsync("AA11111", SearchCriteria.RegistrationNumber);
    }
}