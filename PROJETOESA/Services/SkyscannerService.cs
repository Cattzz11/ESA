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

        public async Task<List<Itinerary>> GetRoundtripAsync(FlightData data)
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

            List<Itinerary> itineraries = new List<Itinerary>();

            if (itinerariesData != null)
            {
                foreach (var item in itinerariesData)
                {
                    var itinerary = new Itinerary
                    {
                        id = item["id"]?.ToString(),
                        price = (double)item["price"]["raw"],
                        isSelfTransfer = (bool)item["isSelfTransfer"],
                        isProtectedSelfTransfer = (bool)item["isProtectedSelfTransfer"],
                        isChangeAllowed = (bool)item["farePolicy"]["isChangeAllowed"],
                        isPartiallyChangeable = (bool)item["farePolicy"]["isPartiallyChangeable"],
                        isCancellationAllowed = (bool)item["farePolicy"]["isCancellationAllowed"],
                        isPartiallyRefundable = (bool)item["farePolicy"]["isPartiallyRefundable"],
                        score = (int)item["score"],
                        trip = new List<Trip>()
                    };

                    var legsData = item["legs"] as JArray;
                    foreach (var legItem in legsData)
                    {
                        var duration = (int)legItem["durationInMinutes"];
                        var trip = new Trip
                        {
                            id = legItem["id"]?.ToString(),
                            durationInMinutes = ConvertMinutesToTimeString(duration),
                            stopCount = (int)legItem["stopCount"],
                            departure = DateTime.Parse(legItem["departure"]?.ToString()),
                            arrival = DateTime.Parse(legItem["arrival"]?.ToString()),
                            timeDeltaInDays = (int)legItem["timeDeltaInDays"],
                            origin = new Place
                            {
                                id = legItem["origin"]["id"]?.ToString(),
                                name = legItem["origin"]["name"]?.ToString(),
                                city = legItem["origin"]["city"]?.ToString(),
                                country = legItem["origin"]["country"]?.ToString()
                            },
                            destination = new Place
                            {
                                id = legItem["destination"]["id"]?.ToString(),
                                name = legItem["destination"]["name"]?.ToString(),
                                city = legItem["destination"]["city"]?.ToString(),
                                country = legItem["destination"]["country"]?.ToString()
                            },
                            carriers = new List<Carrier>(),
                            segments = new List<Segment>()
                        };

                        var marketingCarriers = legItem["carriers"]?["marketing"] as JArray;
                        if (marketingCarriers != null)
                        {
                            foreach (var carrier in marketingCarriers)
                            {
                                var carrierObj = new Carrier
                                {
                                    id = (int)(carrier["id"] ?? 0),
                                    name = (string)(carrier["name"] ?? string.Empty),
                                    logoUrl = (string)(carrier["logoUrl"] ?? string.Empty)
                                };
                                trip.carriers.Add(carrierObj);
                            }
                        }

                        var segmentsData = legItem["segments"] as JArray;
                        if (segmentsData != null)
                        {
                            foreach (var segment in segmentsData)
                            {
                                var segmentObj = new Segment
                                {
                                    origin = new Place
                                    {
                                        id = segment["origin"]["flightPlaceId"]?.ToString(),
                                        name = segment["origin"]["name"]?.ToString(),
                                    },
                                    destination = new Place
                                    {
                                        id = segment["destination"]["flightPlaceId"]?.ToString(),
                                        name = segment["destination"]["name"]?.ToString(),
                                    },
                                    departure = DateTime.Parse(segment["departure"]?.ToString() ?? string.Empty),
                                    arrival = DateTime.Parse(segment["arrival"]?.ToString() ?? string.Empty),
                                    durationInMinutes = segment["durationInMinutes"]?.ToObject<int>() ?? 0,
                                    flightNumber = segment["flightNumber"]?.ToString(),
                                    carrier = new Carrier
                                    {
                                        id = segment["marketingCarrier"]["id"]?.ToObject<int>() ?? 0,
                                        name = segment["marketingCarrier"]["name"]?.ToString(),
                                    }
                                };
                                trip.segments.Add(segmentObj);
                            }
                        }
                        itinerary.trip.Add(trip);
                    }
                    itineraries.Add(itinerary);
                }
            }

            return itineraries;
        }
        
        public async Task<EverywhereDestination> GetEverywhereAsync(FlightData data)
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
            var bucketsArray = jsonObject["data"]["everywhereDestination"]["buckets"] as JArray;
            var resultsArray = jsonObject["data"]["everywhereDestination"]["results"] as JArray;

            EverywhereDestination everywhereDestination = new EverywhereDestination
            {
                Cheapest = new List<Result>(),
                Direct = new List<Result>(),
                Popular = new List<Result>()
            };

            var resultsById = resultsArray.Where(r => r["content"]?["location"]?["id"] != null).ToDictionary(
                r => (string)r["content"]["location"]["id"],
                r => new Result
                {
                    Id = (string)r["content"]["location"]["id"],
                    CodeLocation = (string)r["content"]["location"]["skyCode"],
                    Location = (string)r["content"]["location"]["name"],
                    FlightQuotes = new List<FlightQuotes>
                    {
                        new FlightQuotes
                        {
                            Type = "Cheapest",
                            Price = (string)r["content"]["flightQuotes"]?["cheapest"]?["price"] ?? "Unknown",
                            Direct = (bool?)r["content"]["flightQuotes"]?["cheapest"]?["direct"] ?? false
                        },
                        new FlightQuotes
                        {
                            Type = "Direct",
                            Price = (string)r["content"]["flightQuotes"]?["direct"]?["price"] ?? "Unknown",
                            Direct = (bool?)r["content"]["flightQuotes"]?["direct"]?["direct"] ?? false
                        }
                    },
                    ImageUrl = (string)r["content"]["image"]?["url"] ?? "Unknown"
                }
            );

            foreach (var bucket in bucketsArray)
            {
                var label = (string)bucket["label"];
                var resultIds = bucket["resultIds"].Select(id => ((string)id).Replace("location-", "")).ToList();

                foreach (var resultId in resultIds)
                {
                    if (resultsById.TryGetValue(resultId, out var foundResult))
                    {
                        switch (label)
                        {
                            case "Cheapest flights":
                                everywhereDestination.Cheapest.Add(foundResult);
                                break;
                            case "Direct flights":
                                everywhereDestination.Direct.Add(foundResult);
                                break;
                            case "Suggested for you":
                                everywhereDestination.Popular.Add(foundResult);
                                break;
                            default:
                                Debug.WriteLine("Label desconhecido: " + label);
                                break;
                        }
                    }
                    else
                    {
                        Debug.WriteLine("Resultado para o ID não encontrado: " + resultId);
                    }
                }
            }
            return everywhereDestination;
        }

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

        public async Task<string> GetDataAsync(Country data)
        {
            var response = await _httpClient.GetAsync($"flights/auto-complete?query={data.name}");
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();

            return content;
        }

        public async Task<List<Airport>> GetAirportListAsync(Country country)
        {
            bool countryExists = await _context.Countries.AnyAsync(c => c.id == country.id);

            if (countryExists)
            {
                List<Airport> airportList = await _context.Airport.Where(c => c.countryId == country.id).ToListAsync();
                return airportList;
            }
            else
            {
                var result = await GetDataAsync(country);

                var jsonObject = JObject.Parse(result);
                var data = jsonObject["data"] as JArray;

                List<Airport> airports = new List<Airport>();

                if (data != null)
                {
                    foreach (var item in data)
                    {
                        var entityType = item["navigation"]["entityType"]?.ToString();
                        if (entityType == "AIRPORT")
                        {
                            var airport = new Airport
                            {
                                id = item["navigation"]["relevantFlightParams"]["skyId"]?.ToString(),
                                name = item["presentation"]["title"]?.ToString(),
                                apiKey = item["presentation"]["id"]?.ToString(),
                                countryId = country.id
                            };

                            airports.Add(airport);
                        }
                    }
                }

                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    var newCountry = new Country { id = country.id, name = country.name };
                    await _context.Countries.AddAsync(newCountry);

                    foreach (var item in airports)
                    {
                        var newAirport = new Airport { id = item.id, name = item.name, apiKey = item.apiKey, countryId = item.countryId };
                        await _context.Airport.AddAsync(newAirport);
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                }

                return airports;
            }
        }

        public async Task<List<Itinerary>> GetSugestionsCompanyAsync(string carrier)
        {
            string tomorrow = DateTime.Now.AddDays(1).ToString("yyyy-MM-dd");
            string nextWeek = DateTime.Now.AddDays(8).ToString("yyyy-MM-dd");
            FlightData origin = new FlightData { fromEntityId = "eyJlIjoiOTU1NjUwNTUiLCJzIjoiTElTIiwiaCI6IjI3NTQ0MDcyIn0=" };

            // Lista de possiveis viagens
            EverywhereDestination possibleDestinations = await GetEverywhereAsync(origin);

            // Lista de paises possiveis
            List<Country> paises = possibleDestinations.Cheapest
                .Concat(possibleDestinations.Direct)
                .Concat(possibleDestinations.Popular)
                .Select(destination => new Country { id = destination.CodeLocation, name = destination.Location })
                .Distinct()
                .ToList();

            List<Itinerary> finalItineraries = new List<Itinerary>();
            while (finalItineraries.Count < 1)
            {
                Country countrySelected = SelectRandomCountry(paises);
                List<Airport> airports = await GetAirportListAsync(countrySelected);
                Airport selectedAirport = SelectRandomAirport(airports);

                FlightData data = new FlightData { fromEntityId = origin.fromEntityId, toEntityId = selectedAirport.apiKey, departDate = tomorrow, returnDate = nextWeek };

                List<Itinerary> itineraries = await GetRoundtripAsync(data);

                var foundItineraries = itineraries
                    .Where(itinerary => itinerary.trip
                    .All(trip => trip.carriers
                    .All(c => c.name == carrier)))
                    .Take(2);

                finalItineraries.AddRange(foundItineraries);
            }

            return finalItineraries;
        }

        public async Task<List<Carrier>> GetFavoriteAirlineAsync()
        {
            List<Carrier> carrierList = await _context.Carrier.OrderByDescending(e => e.searchTimes).Take(5).ToListAsync();
            
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

        private Airport SelectRandomAirport(List<Airport> airports)
        {
            var randomIndex = _random.Next(airports.Count);
            return airports[randomIndex];
        }
    }


}