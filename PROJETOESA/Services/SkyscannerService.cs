using Newtonsoft.Json.Linq;
using PROJETOESA.Controllers;
using PROJETOESA.Models;
using System.Diagnostics;
using Newtonsoft.Json.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;
using PROJETOESA.Data;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.Metrics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Mono.TextTemplating;

namespace PROJETOESA.Services
{
    public class SkyscannerService
    {
        private readonly HttpClient _httpClient;
        private readonly AeroHelperContext _context;
        private readonly Random _random = new Random();

        public SkyscannerService(IHttpClientFactory httpClientFactory, AeroHelperContext context)
        {
            _httpClient = httpClientFactory.CreateClient("SkyscannerAPI");
            _context = context;
        }

        public async Task<List<Flight>> GetRoundtripAsync(FlightData data)
        {
            var queryParams = new List<string>();

            queryParams.Add($"fromEntityId={data.fromEntityId}");
            queryParams.Add($"toEntityId={data.toEntityId}");
            queryParams.Add($"departDate={data.departDate}");
            queryParams.Add($"returnDate={data.returnDate}");
            if (!string.IsNullOrEmpty(data.market)) queryParams.Add($"market={data.market}");
            if (!string.IsNullOrEmpty(data.locale)) queryParams.Add($"locale={data.locale}");
            if (!string.IsNullOrEmpty(data.currency)) queryParams.Add($"currency={data.currency}");
            if (data.Adults.HasValue) queryParams.Add($"adults={data.Adults}");
            if (data.Children.HasValue) queryParams.Add($"children={data.Children}");
            if (data.Infants.HasValue) queryParams.Add($"infants={data.Infants}");
            if (!string.IsNullOrEmpty(data.cabinClass)) queryParams.Add($"cabinClass={data.cabinClass}");

            var queryString = string.Join("&", queryParams);

            var response = await _httpClient.GetAsync($"flights/search-roundtrip?{queryString}");
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();

            // Organizar os dados
            var jsonObject = JObject.Parse(content);
            var itinerariesData = jsonObject["data"]["itineraries"] as JArray;

            List<Flight> flights = new List<Flight>();

            if (itinerariesData != null)
            {
                foreach (var itinerary in itinerariesData)
                {
                    var legs = itinerary["legs"] as JArray;
                    if (legs != null)
                    {
                        foreach (var leg in legs)
                        {
                            City originCity = await GetCityAsync(leg["origin"]["id"]?.ToString());
                            City destinationCity = await GetCityAsync(leg["destination"]["id"]?.ToString());

                            var flight = new Flight
                            {
                                Id = itinerary["id"]?.ToString(),
                                Price = (double?)itinerary["price"]?["raw"] ?? 0.0,
                                Departure = DateTime.Parse(leg["departure"]?.ToString()),
                                Arrival = DateTime.Parse(leg["arrival"]?.ToString()),
                                Duration = ConvertMinutesToTimeString(leg["durationInMinutes"].ToObject<int>()),
                                OriginCityId = leg["origin"]["id"]?.ToString(),
                                DestinationCityId = leg["destination"]["id"]?.ToString(),
                                OriginCity = originCity,
                                DestinationCity = destinationCity,
                                isSelfTransfer = itinerary["isSelfTransfer"]?.ToObject<bool>() ?? false,
                                isProtectedSelfTransfer = itinerary["isProtectedSelfTransfer"]?.ToObject<bool>() ?? false,
                                isChangeAllowed = itinerary["farePolicy"]?["isChangeAllowed"]?.ToObject<bool>() ?? false,
                                isPartiallyChangeable = itinerary["farePolicy"]?["isPartiallyChangeable"]?.ToObject<bool>() ?? false,
                                isCancellationAllowed = itinerary["farePolicy"]?["isCancellationAllowed"]?.ToObject<bool>() ?? false,
                                isPartiallyRefundable = itinerary["farePolicy"]?["isPartiallyRefundable"]?.ToObject<bool>() ?? false,
                                Score = itinerary["score"]?.ToObject<double>() ?? 0,
                                Segments = new List<Segment>()
                            };

                            var carriersObj = leg["carriers"] as JObject;
                            var marketingCarriers = carriersObj?["marketing"] as JArray;

                            if (marketingCarriers != null)
                            {
                                foreach (var car in marketingCarriers)
                                {
                                    var carId = car["id"]?.ToString();

                                    if (!await _context.Carrier.AnyAsync(c => c.Id == carId))
                                    {
                                        var newCarrier = new Carrier
                                        {
                                            Id = carId,
                                            Name = car["name"]?.ToString(),
                                            LogoURL = car["logoUrl"]?.ToString()
                                        };

                                        await _context.Carrier.AddAsync(newCarrier);
                                        await _context.SaveChangesAsync();
                                    }
                                }
                            }

                            var segments = leg["segments"] as JArray;
                            if (segments != null)
                            {
                                foreach (var segment in segments)
                                {
                                    var item = new Segment
                                    {
                                        FlightNumber = segment["flightNumber"]?.ToString(),
                                        Departure = DateTime.Parse(segment["departure"]?.ToString()),
                                        Arrival = DateTime.Parse(segment["arrival"]?.ToString()),
                                        Duration = ConvertMinutesToTimeString(segment["durationInMinutes"].ToObject<int>()),
                                        FlightId = itinerary["id"]?.ToString(),
                                        CarrierId = segment["marketingCarrier"]["id"]?.ToString(),
                                    };

                                    flight.Segments.Add(item);
                                }
                            }

                            flights.Add(flight);
                        }
                    }
                }
            }
            return flights;
        }

        private async Task<City> GetCityAsync(string cityId)
        {
            return await _context.City.FirstOrDefaultAsync(c => c.Id == cityId);
        }

        public async Task<List<Country>> GetEverywhereAsync(FlightData data)
        {
            var queryParams = new List<string>();

            queryParams.Add($"fromEntityId={data.fromEntityId}");
            if (!string.IsNullOrEmpty(data.toEntityId)) queryParams.Add($"toEntityId={data.toEntityId}");
            if (data.year.HasValue) queryParams.Add($"year={data.year}");
            if (data.month.HasValue) queryParams.Add($"month={data.month}");
            if (!string.IsNullOrEmpty(data.market)) queryParams.Add($"market={data.market}");
            if (!string.IsNullOrEmpty(data.locale)) queryParams.Add($"locale={data.locale}");
            if (!string.IsNullOrEmpty(data.currency)) queryParams.Add($"currency={data.currency}");
            if (data.Adults.HasValue) queryParams.Add($"adults={data.Adults}");
            if (data.Children.HasValue) queryParams.Add($"children={data.Children}");
            if (data.Infants.HasValue) queryParams.Add($"infants={data.Infants}");
            if (!string.IsNullOrEmpty(data.cabinClass)) queryParams.Add($"cabinClass={data.cabinClass}");

            var queryString = string.Join("&", queryParams);
            var response = await _httpClient.GetAsync($"flights/search-everywhere?{queryString}&placeTypes=AIRPORT");
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();

            // Organizar os dados
            var jsonObject = JObject.Parse(content);
            var resultsArray = jsonObject["data"]["everywhereDestination"]["results"] as JArray;

            var countriesDict = new Dictionary<string, Country>();

            foreach (var result in resultsArray)
            {
                var codeLocation = (string)result["content"]["location"]["skyCode"];
                var location = (string)result["content"]["location"]["name"];

                if (!countriesDict.ContainsKey(codeLocation))
                {

                    var country = await _context.Country.Include(c => c.Cities).FirstOrDefaultAsync(c => c.Id == codeLocation);

                    if (country == null)
                    {
                        country = new Country
                        {
                            Id = codeLocation,
                            Name = location
                        };
                    }

                    countriesDict[codeLocation] = country;
                }
            }

            return countriesDict.Values.ToList();
        }

        // Falta depois Organizar como se recebe os dados
        public async Task<string> GetOneWayAsync(FlightData data)
        {
            var queryParams = new List<string>();

            queryParams.Add($"fromEntityId={data.fromEntityId}");
            queryParams.Add($"toEntityId={data.toEntityId}");
            queryParams.Add($"departDate={data.departDate}");
            if (!string.IsNullOrEmpty(data.market)) queryParams.Add($"market={data.market}");
            if (!string.IsNullOrEmpty(data.locale)) queryParams.Add($"locale={data.locale}");
            if (!string.IsNullOrEmpty(data.currency)) queryParams.Add($"currency={data.currency}");
            if (data.Adults.HasValue) queryParams.Add($"adults={data.Adults}");
            if (data.Children.HasValue) queryParams.Add($"children={data.Children}");
            if (data.Infants.HasValue) queryParams.Add($"infants={data.Infants}");
            if (!string.IsNullOrEmpty(data.cabinClass)) queryParams.Add($"cabinClass={data.cabinClass}");

            var queryString = string.Join("&", queryParams);
            var response = await _httpClient.GetAsync($"flights/search-one-way?{queryString}");
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            return content;
        }

        // Falta depois Organizar como se recebe os dados
        public async Task<string> GetCalendarAsync(FlightData data)
        {
            var queryParams = new List<string>();

            queryParams.Add($"fromEntityId={data.fromEntityId}");
            queryParams.Add($"departDate={data.departDate}");
            if (!string.IsNullOrEmpty(data.toEntityId)) queryParams.Add($"toEntityId={data.toEntityId}");
            if (!string.IsNullOrEmpty(data.market)) queryParams.Add($"market={data.market}");
            if (!string.IsNullOrEmpty(data.locale)) queryParams.Add($"locale={data.locale}");
            if (!string.IsNullOrEmpty(data.currency)) queryParams.Add($"currency={data.currency}");

            var queryString = string.Join("&", queryParams);
            var response = await _httpClient.GetAsync($"flights/price-calendar?{queryString}");
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            return content;
        }

        public async Task<List<CustomGetDataModel>> GetDataAsync(Country data)
        {
            var response = await _httpClient.GetAsync($"flights/auto-complete?query={data.Name}");
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();

            var jsonObject = JObject.Parse(content);
            var datajson = jsonObject["data"] as JArray;

            List<CustomGetDataModel> customData = new List<CustomGetDataModel>();

            if (datajson != null)
            {
                foreach (var item in datajson)
                {                  
                    var airport = new CustomGetDataModel
                    {
                        id = item["presentation"]["id"]?.ToString(),
                        skyId = item["navigation"]["relevantFlightParams"]["skyId"]?.ToString(),
                        localizedName = item["navigation"]["localizedName"]?.ToString(),
                        flightPlaceType = item["navigation"]["relevantFlightParams"]["flightPlaceType"]?.ToString(),
                        subtitle = item["presentation"]["subtitle"]?.ToString(),
                    };
                    
                    customData.Add(airport);
                }
                
            }

            return customData;
        }

        public async Task<List<City>> GetAirportListAsync(Country country)
        {
            bool countryExists = await _context.Country.AnyAsync(c => c.Id == country.Id);

            if (countryExists)
            {
                return await _context.City.Where(c => c.CountryId == country.Id).ToListAsync();
            }
            else
            {
                var result = await GetDataAsync(country);

                List<City> cities = new List<City>();

                foreach (var item in result)
                {
                    if(item.flightPlaceType != "COUNTRY")
                    {

                        var airport = new City
                        {
                            Id = item.skyId,
                            Name = item.localizedName,
                            ApiKey = item.id,
                            CountryId = country.Id,
                        };
                    }
                }

                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    var newCountry = new Country { Id = country.Id, Name = country.Name };
                    await _context.Country.AddAsync(newCountry);

                    foreach (var item in cities)
                    {
                        var newCity = new City { Id = item.Id, Name = item.Name, ApiKey = item.ApiKey, CountryId = item.CountryId };
                        await _context.City.AddAsync(newCity);
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                }

                return cities;
            }
        }

        public async Task<List<Flight>> GetSugestionsCompanyAsync(string carrier)
        {
            string tomorrow = DateTime.Now.AddDays(1).ToString("yyyy-MM-dd");
            string nextWeek = DateTime.Now.AddDays(8).ToString("yyyy-MM-dd");
            FlightData origin = new FlightData { fromEntityId = "eyJlIjoiOTU1NjUwNTUiLCJzIjoiTElTIiwiaCI6IjI3NTQ0MDcyIn0=" };

            List<Country> possibleDestinations = await GetEverywhereAsync(origin);
            
            List<Flight> finalItineraries = new List<Flight>();
            while (finalItineraries.Count < 1)
            {
                Country countrySelected = SelectRandomCountry(possibleDestinations);

                List<City> airports;
                if(!countrySelected.Cities.Any())
                {
                    airports = await GetAirportListAsync(countrySelected);
                }
                else
                {
                    airports = countrySelected.Cities;
                }

                City selectedAirport = SelectRandomAirport(airports);

                FlightData data = new FlightData { fromEntityId = origin.fromEntityId, toEntityId = selectedAirport.ApiKey, departDate = tomorrow, returnDate = nextWeek };

                List<Flight> itineraries = await GetRoundtripAsync(data);

                List<Flight> foundItineraries = itineraries
                    .Where(itinerary => itinerary.Segments
                    .All(segment => segment.CarrierId == carrier))
                    .Take(2)
                    .ToList();

                finalItineraries.AddRange(foundItineraries);
            }

            return finalItineraries;
        }

        public async Task<List<Carrier>> GetFavoriteAirlineAsync()
        {
            List<Carrier> carrierList = await _context.Carrier.OrderByDescending(e => e.SearchTimes).Take(5).ToListAsync();

            return carrierList;
        }

        private string ConvertMinutesToTimeString(int durationInMinutes)
        {
            int hours = durationInMinutes / 60;
            int minutes = durationInMinutes % 60;
            return $"{hours:00}:{minutes:00}";
        }

        private Country SelectRandomCountry(List<Country> countries)
        {
            var randomIndex = _random.Next(countries.Count);
            Country selectedCountry = countries[randomIndex];
            countries.RemoveAt(randomIndex);
            return selectedCountry;
        }

        private City SelectRandomAirport(List<City> cities)
        {
            var randomIndex = _random.Next(cities.Count);
            return cities[randomIndex];
        }
    }

    public class CustomGetDataModel
    {
        public string id { get; set; }
        public string localizedName { get; set; }
        public string flightPlaceType { get; set; }
        public string subtitle { get; set; }
        public string skyId { get; set; }
  

    }
}