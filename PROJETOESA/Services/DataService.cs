using Microsoft.EntityFrameworkCore;
using PROJETOESA.Models;
using static PROJETOESA.Services.SkyscannerService;
using System.Diagnostics;
using System.Net.Http;
using PROJETOESA.Data;
using System.Text.Json;

namespace PROJETOESA.Services
{
    public class DataService
    {
        private readonly HttpClient _httpClient;
        private readonly AeroHelperContext _context;
        private readonly HttpClient _httpClient2;
        private readonly SkyscannerService _skyscannerService;

        public DataService(
            IHttpClientFactory httpClientFactory, 
            AeroHelperContext context, 
            IHttpClientFactory clientFactory,
            SkyscannerService skyscannerService
            )
        {
            _httpClient = httpClientFactory.CreateClient("SkyscannerAPI");
            _context = context;
            _httpClient2 = clientFactory.CreateClient("CountriesAPI");
            _skyscannerService = skyscannerService;
        }

        public async Task<List<TripDto>> GetFlightsByUserAsync(string userId)
        {
            var tripIds = await _context.UserFlight
                                .Where(uf => uf.UserId == userId)
                                .Select(uf => uf.TripId)
                                .ToListAsync();

            var trips = await _context.Trip
                                .Where(t => tripIds.Contains(t.Id))
                                .Include(t => t.Flights)
                                    .ThenInclude(f => f.Segments)
                                        .ThenInclude(s => s.OriginCity)
                                            .ThenInclude(city => city.Country)
                                .Include(t => t.Flights)
                                    .ThenInclude(f => f.Segments)
                                        .ThenInclude(s => s.DestinationCity)
                                            .ThenInclude(city => city.Country)
                                .Include(t => t.Flights)
                                    .ThenInclude(f => f.Segments)
                                        .ThenInclude(s => s.Carrier)
                                .ToListAsync();

            var tripDtos = trips.Select(t => new TripDto
            {
                Id = t.Id,
                Price = t.Price,
                Flights = t.Flights.Select(f => new FlightDto
                {
                    Id = f.Id,
                    Duration = f.Duration,
                    Segments = f.Segments.Select(s => new SegmentDto
                    {
                        FlightNumber = s.FlightNumber,
                        Departure = s.Departure,
                        Arrival = s.Arrival,
                        Duration = s.Duration,
                        OriginCity = new CityDto
                        {
                            Id = s.OriginCity.Id,
                            Name = s.OriginCity.Name,
                            Country = new CountryDto
                            {
                                Id = s.OriginCity.Country.Id,
                                Name = s.OriginCity.Country.Name,
                            }
                        },
                        DestinationCity = new CityDto
                        {
                            Id = s.DestinationCity.Id,
                            Name = s.DestinationCity.Name,
                            Country = new CountryDto
                            {
                                Id = s.DestinationCity.Country.Id,
                                Name = s.DestinationCity.Country.Name,
                            }
                        },
                        Carrier = s.Carrier != null ? new CarrierDto
                        {
                            Id = s.Carrier.Id,
                            Name = s.Carrier.Name,
                            LogoURL = s.Carrier.LogoURL,
                            SearchTimes = s.Carrier.SearchTimes,
                        } : null
                    }).ToList()
                }).ToList()
            }).ToList();

            return tripDtos;
        }

        public async Task<List<CityDto>> GetAllCitiesAsync()
        {
            var citiesWithCountries = await _context.City
            .Include(c => c.Country)
            .Select(c => new CityDto
            {
                Id = c.Id,
                Name = c.Name,
                ApiKey = c.ApiKey,
                Country = new CountryDto
                {
                    Id = c.Country.Id,
                    Name = c.Country.Name
                }
            }).ToListAsync();

            return citiesWithCountries;
        }

        // Função apenas com o objetivo de preencher a tabela de Países
        public async Task<List<Country>> PopulateBDCountries()
        {
            _httpClient2.Timeout = TimeSpan.FromMinutes(2);
            var response = await _httpClient2.GetAsync("v3.1/all");
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var countryJsonModels = JsonSerializer.Deserialize<List<CountryJsonModel>>(content);

            var countries = countryJsonModels.Select(c => new Country
            {
                Id = c.cca2,
                Name = c.name.common
            }).ToList();
            bool insert = false;

            if (countries != null)
            {
                foreach (var item in countries)
                {
                    var countryLocal = await _context.Country.FirstOrDefaultAsync(c => c.Id == item.Id);
                    if (countryLocal == null)
                    {
                        await _context.Country.AddAsync(item);
                        insert = true;
                    }
                }
                if (insert)
                {
                    await _context.SaveChangesAsync();
                }
            }
            return countries;
        }

        // Função apenas para preencher a tabela de Cidades
        public async Task<List<City>> PopulateBDCity()
        {
            List<Country> countries = await _context.Country.ToListAsync();
            List<City> cities = new List<City>();
            int count = 0;
            foreach (var country in countries)
            {
                cities.AddRange(await PopulateBDCityAux(country));
                count++;
            }
            return cities;
        }

        // Função apenas para preencher a tabela de Cidades
        private async Task<List<City>> PopulateBDCityAux(Country country)
        {
            var existingCities = await _context.City.Where(c => c.CountryId == country.Id).ToListAsync();
            if (existingCities.Any())
            {
                return existingCities;
            }
            else
            {
                var result = await _skyscannerService.GetDataAsync(country);
                List<City> cities = new List<City>();

                foreach (var item in result)
                {
                    if (item.flightPlaceType != "COUNTRY" && !existingCities.Any(c => c.Id == item.skyId))
                    {
                        var city = new City
                        {
                            Id = item.skyId,
                            Name = item.localizedName,
                            ApiKey = item.id,
                            CountryId = country.Id
                        };
                        cities.Add(city);
                    }
                }

                if (cities.Any())
                {
                    using var transaction = await _context.Database.BeginTransactionAsync();
                    try
                    {
                        foreach (var item in cities)
                        {
                            var existingCity = await _context.City.FindAsync(item.Id);
                            if (existingCity == null)
                            {
                                var newCity = new City { Id = item.Id, Name = item.Name, ApiKey = item.ApiKey, CountryId = item.CountryId, Country = item.Country };
                                await _context.City.AddAsync(newCity);
                            }
                        }

                        await _context.SaveChangesAsync();
                        await transaction.CommitAsync();
                    }
                    catch (Exception)
                    {
                        await transaction.RollbackAsync();
                        throw;
                    }
                }
                return cities;
            }
        }

        private class CountryJsonModel
        {
            public string cca2 { get; set; }
            public Name name { get; set; }
        }

        private class Name
        {
            public string common { get; set; }
        }
    }    
}
