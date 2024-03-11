using Newtonsoft.Json.Linq;
using PROJETOESA.Models;
using System.Diagnostics;
using PROJETOESA.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Text.Json;
using System.Diagnostics.Metrics;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Mvc;

namespace PROJETOESA.Services
{
    public class SkyscannerService
    {
        private readonly HttpClient _httpClient;
        private readonly AeroHelperContext _context;
        private readonly Random _random = new Random();

        public SkyscannerService(IHttpClientFactory httpClientFactory, AeroHelperContext context, IHttpClientFactory clientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("SkyscannerAPI");
            _context = context;
        }

        public async Task<List<Trip>> GetRoundtripAsync(FlightData data)
        {
            var queryParams = new List<string>();

            queryParams.Add($"fromEntityId={data.fromEntityId}");
            queryParams.Add($"toEntityId={data.toEntityId}");
            queryParams.Add($"departDate={data.departDate}");
            queryParams.Add($"returnDate={data.returnDate}");
            if (!string.IsNullOrEmpty(data.market)) queryParams.Add($"market={data.market}");
            if (!string.IsNullOrEmpty(data.locale)) queryParams.Add($"locale={data.locale}");
            if (!string.IsNullOrEmpty(data.currency))
            {
                queryParams.Add($"currency={data.currency}");
            }
            else
            {
                queryParams.Add($"currency=EUR");
            }
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

            List<Trip> trips = new List<Trip>();

            if (itinerariesData != null)
            {
                foreach (var itinerary in itinerariesData)
                {
                    Trip trip = new Trip
                    {
                        Id = itinerary["id"]?.ToString(),
                        Price = (double?)itinerary["price"]?["raw"] ?? 0.0,
                        isSelfTransfer = itinerary["isSelfTransfer"]?.ToObject<bool>() ?? false,
                        isProtectedSelfTransfer = itinerary["isProtectedSelfTransfer"]?.ToObject<bool>() ?? false,
                        isChangeAllowed = itinerary["farePolicy"]?["isChangeAllowed"]?.ToObject<bool>() ?? false,
                        isPartiallyChangeable = itinerary["farePolicy"]?["isPartiallyChangeable"]?.ToObject<bool>() ?? false,
                        isCancellationAllowed = itinerary["farePolicy"]?["isCancellationAllowed"]?.ToObject<bool>() ?? false,
                        isPartiallyRefundable = itinerary["farePolicy"]?["isPartiallyRefundable"]?.ToObject<bool>() ?? false,
                        Score = itinerary["score"]?.ToObject<double>() ?? 0,
                        Flights = new List<Flight>()
                    };

                    var legs = itinerary["legs"] as JArray;
                    if (legs != null)
                    {
                        foreach (var leg in legs)
                        {
                            City originCity = await GetCityAsync(leg["origin"]["id"]?.ToString());
                            City destinationCity = await GetCityAsync(leg["destination"]["id"]?.ToString());

                            var flight = new Flight
                            {
                                Id = leg["id"]?.ToString(),
                                Duration = ConvertMinutesToTimeString(leg["durationInMinutes"].ToObject<int>()),
                                Departure = DateTime.Parse(leg["departure"]?.ToString()),
                                Arrival = DateTime.Parse(leg["arrival"]?.ToString()),
                                OriginCityId = leg["origin"]["id"]?.ToString(),
                                DestinationCityId = leg["destination"]["id"]?.ToString(),
                                OriginCity = originCity,
                                DestinationCity = destinationCity,
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
                                    City originCitySegment = await GetCityAsync(segment["origin"]["flightPlaceId"]?.ToString());
                                    City destinationCitySegment = await GetCityAsync(segment["destination"]["flightPlaceId"]?.ToString());
                                    Carrier carrierSegment = await GetCarrierAsync(segment["marketingCarrier"]["id"]?.ToString());

                                    var item = new Segment
                                    {
                                        FlightNumber = segment["flightNumber"]?.ToString(),
                                        Departure = DateTime.Parse(segment["departure"]?.ToString()),
                                        Arrival = DateTime.Parse(segment["arrival"]?.ToString()),
                                        Duration = ConvertMinutesToTimeString(segment["durationInMinutes"].ToObject<int>()),
                                        FlightId = leg["id"]?.ToString(),
                                        CarrierId = segment["marketingCarrier"]["id"]?.ToString(),
                                        OriginCityId = segment["origin"]["flightPlaceId"]?.ToString(),
                                        DestinationCityId = segment["destination"]["flightPlaceId"]?.ToString(),
                                        OriginCity = originCitySegment,
                                        DestinationCity = destinationCitySegment,
                                        Carrier = carrierSegment,
                                    };

                                    flight.Segments.Add(item);
                                }
                            }

                            trip.Flights.Add(flight);
                        }
                    }

                    trips.Add(trip);
                }
            }
            return trips;
        }

        public async Task<double[]> GetRoundtripPricesAsync(FlightData data)
        {
            var queryParams = new List<string>();

            queryParams.Add($"fromEntityId={data.fromEntityId}");
            queryParams.Add($"toEntityId={data.toEntityId}");
            queryParams.Add($"departDate={data.departDate}");
            queryParams.Add($"returnDate={data.returnDate}");
            if (!string.IsNullOrEmpty(data.market)) queryParams.Add($"market={data.market}");
            if (!string.IsNullOrEmpty(data.locale)) queryParams.Add($"locale={data.locale}");
            if (!string.IsNullOrEmpty(data.currency))
            {
                queryParams.Add($"currency={data.currency}");
            }
            else
            {
                queryParams.Add($"currency=EUR");
            }
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

            List<Trip> trips = new List<Trip>();

            List<double> prices = new List<double>();

            if (itinerariesData != null)
            {
                foreach (var itinerary in itinerariesData)
                {
                    double price = (double?)itinerary["price"]?["raw"] ?? 0.0;
                    prices.Add(price);

                    Trip trip = new Trip
                    {
                        Id = itinerary["id"]?.ToString(),
                        Price = (double?)itinerary["price"]?["raw"] ?? 0.0,
                        isSelfTransfer = itinerary["isSelfTransfer"]?.ToObject<bool>() ?? false,
                        isProtectedSelfTransfer = itinerary["isProtectedSelfTransfer"]?.ToObject<bool>() ?? false,
                        isChangeAllowed = itinerary["farePolicy"]?["isChangeAllowed"]?.ToObject<bool>() ?? false,
                        isPartiallyChangeable = itinerary["farePolicy"]?["isPartiallyChangeable"]?.ToObject<bool>() ?? false,
                        isCancellationAllowed = itinerary["farePolicy"]?["isCancellationAllowed"]?.ToObject<bool>() ?? false,
                        isPartiallyRefundable = itinerary["farePolicy"]?["isPartiallyRefundable"]?.ToObject<bool>() ?? false,
                        Score = itinerary["score"]?.ToObject<double>() ?? 0,
                        Flights = new List<Flight>()
                    };

                    var legs = itinerary["legs"] as JArray;
                    if (legs != null)
                    {
                        foreach (var leg in legs)
                        {
                            City originCity = await GetCityAsync(leg["origin"]["id"]?.ToString());
                            City destinationCity = await GetCityAsync(leg["destination"]["id"]?.ToString());

                            var flight = new Flight
                            {
                                Id = leg["id"]?.ToString(),
                                Duration = ConvertMinutesToTimeString(leg["durationInMinutes"].ToObject<int>()),
                                Departure = DateTime.Parse(leg["departure"]?.ToString()),
                                Arrival = DateTime.Parse(leg["arrival"]?.ToString()),
                                OriginCityId = leg["origin"]["id"]?.ToString(),
                                DestinationCityId = leg["destination"]["id"]?.ToString(),
                                OriginCity = originCity,
                                DestinationCity = destinationCity,
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
                                    City originCitySegment = await GetCityAsync(segment["origin"]["flightPlaceId"]?.ToString());
                                    City destinationCitySegment = await GetCityAsync(segment["destination"]["flightPlaceId"]?.ToString());
                                    Carrier carrierSegment = await GetCarrierAsync(segment["marketingCarrier"]["id"]?.ToString());

                                    var item = new Segment
                                    {
                                        FlightNumber = segment["flightNumber"]?.ToString(),
                                        Departure = DateTime.Parse(segment["departure"]?.ToString()),
                                        Arrival = DateTime.Parse(segment["arrival"]?.ToString()),
                                        Duration = ConvertMinutesToTimeString(segment["durationInMinutes"].ToObject<int>()),
                                        FlightId = leg["id"]?.ToString(),
                                        CarrierId = segment["marketingCarrier"]["id"]?.ToString(),
                                        OriginCityId = segment["origin"]["flightPlaceId"]?.ToString(),
                                        DestinationCityId = segment["destination"]["flightPlaceId"]?.ToString(),
                                        OriginCity = originCitySegment,
                                        DestinationCity = destinationCitySegment,
                                        Carrier = carrierSegment,
                                    };

                                    flight.Segments.Add(item);
                                }
                            }

                            trip.Flights.Add(flight);
                        }
                    }

                    trips.Add(trip);
                }
            }

            return prices.ToArray();
        }


        private async Task<City> GetCityAsync(string cityId)
        {
            return await _context.City.FirstOrDefaultAsync(c => c.Id == cityId);
        }

        private async Task<Carrier> GetCarrierAsync(string carrierId)
        {
            return await _context.Carrier.FirstOrDefaultAsync(c => c.Id == carrierId);
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
            if (!string.IsNullOrEmpty(data.currency))
            {
                queryParams.Add($"currency={data.currency}");
            }
            else
            {
                queryParams.Add($"currency=EUR");
            }
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

                    var country = await _context.Country.FirstOrDefaultAsync(c => c.Id == codeLocation);

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
            if (!string.IsNullOrEmpty(data.currency))
            {
                queryParams.Add($"currency={data.currency}");
            }
            else
            {
                queryParams.Add($"currency=EUR");
            }
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
        public async Task<List<Calendar>> GetCalendarAsync(FlightData data)
        {
            var queryParams = new List<string>();

            string today = DateTime.Now.ToString("yyyy-MM-dd");

            queryParams.Add($"fromEntityId={data.fromEntityId}");
            queryParams.Add($"departDate={today}");
            if (!string.IsNullOrEmpty(data.toEntityId)) queryParams.Add($"toEntityId={data.toEntityId}");
            if (!string.IsNullOrEmpty(data.market)) queryParams.Add($"market={data.market}");
            if (!string.IsNullOrEmpty(data.locale)) queryParams.Add($"locale={data.locale}");
            if (!string.IsNullOrEmpty(data.currency))
            {
                queryParams.Add($"currency={data.currency}");
            }
            else
            {
                queryParams.Add($"currency=EUR");
            }

            var queryString = string.Join("&", queryParams);
            var response = await _httpClient.GetAsync($"flights/price-calendar?{queryString}");
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();

            var rootObject = JObject.Parse(content);

            var days = rootObject["data"]["flights"]["days"].ToObject<List<JObject>>();

            List<Calendar> calendars = new List<Calendar>();
            foreach (var day in days)
            {
                calendars.Add(new Calendar
                {
                    Date = DateTime.Parse(day["day"].ToString()),
                    Category = day["group"].ToString(),
                    Price = (double)day["price"]
                });
            }

            return calendars;
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
                    if (item.flightPlaceType != "COUNTRY")
                    {

                        var airport = new City
                        {
                            Id = item.skyId,
                            Name = item.localizedName,
                            ApiKey = item.id,
                            CountryId = country.Id,
                            Country = country
                        };
                        cities.Add(airport);
                    }
                }

                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    var newCountry = new Country { Id = country.Id, Name = country.Name };
                    var countryLocal = await _context.Country.FirstOrDefaultAsync(c => c.Id == newCountry.Id);
                    
                    if (countryLocal == null)
                    {
                        await _context.Country.AddAsync(newCountry);
                    }

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

        public async Task<List<Trip>> GetSugestionsCompanyAsyncTest()
        {
            List<Trip> trips = new List<Trip>();

            Trip trip = new Trip
            {
                Id = "13577-2402250945--31781-0-11469-2402251030|11469-2403051635--31781-0-13577-2403051725",
                Price = 84.99,
                isSelfTransfer = false,
                isProtectedSelfTransfer = false,
                isChangeAllowed = false,
                isPartiallyChangeable = false,
                isCancellationAllowed = false,
                isPartiallyRefundable = false,
                Score = 0.999,
                Flights = new List<Flight>()
            };

            var flight1 = new Flight
            {
                Id = "13577-2402250945--31781-0-11469-2402251030",
                Duration = "00:45",
                Departure = new DateTime(2024, 02, 25, 09, 45, 00),
                Arrival = new DateTime(2024, 02, 25, 10, 30, 00),
                OriginCityId = "LIS",
                DestinationCityId = "FAO",
                OriginCity = await GetCityAsync("LIS"),
                DestinationCity = await GetCityAsync("FAO"),
                Segments = new List<Segment>()
            };

            var flight1Segment1 = new Segment
            {
                FlightNumber = "1901",
                Departure = new DateTime(2024, 02, 25, 09, 45, 00),
                Arrival = new DateTime(2024, 02, 25, 10, 30, 00),
                Duration = "00:45",
                FlightId = flight1.Id,
                CarrierId = "-31781",
                OriginCityId = "LIS",
                DestinationCityId = "FAO",
                OriginCity = await GetCityAsync("LIS"),
                DestinationCity = await GetCityAsync("FAO"),
                Carrier = await GetCarrierAsync("-31781"),
            };

            flight1.Segments.Add(flight1Segment1);

            var flight2 = new Flight
            {
                Id = "11469-2403051635--31781-0-13577-2403051725",
                Duration = "00:50",
                Departure = new DateTime(2024, 03, 05, 16, 35, 00),
                Arrival = new DateTime(2024, 03, 05, 17, 25, 00),
                OriginCityId = "FAO",
                DestinationCityId = "LIS",
                OriginCity = await GetCityAsync("FAO"),
                DestinationCity = await GetCityAsync("LIS"),
                Segments = new List<Segment>()
            };

            var flight2Segment1 = new Segment
            {
                FlightNumber = "1904",
                Duration = "00:50",
                Departure = new DateTime(2024, 03, 05, 16, 35, 00),
                Arrival = new DateTime(2024, 03, 05, 17, 25, 00),
                OriginCityId = "FAO",
                DestinationCityId = "LIS",
                OriginCity = await GetCityAsync("FAO"),
                DestinationCity = await GetCityAsync("LIS"),
                FlightId = flight2.Id,
                CarrierId = "-31781",
                Carrier = await GetCarrierAsync("-31781"),
            };

            flight2.Segments.Add(flight2Segment1);

            trip.Flights.Add(flight1);
            trip.Flights.Add(flight2);

            trips.Add(trip);

            return trips;
        }


        public async Task<List<Trip>> GetSugestionsCompanyAsync(string carrierId)
        {
            string tomorrow = DateTime.Now.AddDays(1).ToString("yyyy-MM-dd");
            string nextWeek = DateTime.Now.AddDays(8).ToString("yyyy-MM-dd");
            FlightData origin = new FlightData { fromEntityId = "eyJlIjoiOTU1NjUwNTUiLCJzIjoiTElTIiwiaCI6IjI3NTQ0MDcyIn0=" };

            List<Country> possibleDestinations = await GetEverywhereAsync(origin);

            List<Trip> finalItineraries = new List<Trip>();
            while (finalItineraries.Count < 1)
            {
                Country countrySelected = SelectRandomCountry(possibleDestinations);

                List<City> cities = await _context.City.Where(c => c.CountryId == countrySelected.Id).ToListAsync();

                if (!cities.Any())
                {
                    cities = await GetAirportListAsync(countrySelected);
                }

                City selectedAirport = SelectRandomAirport(cities);

                FlightData data = new FlightData { fromEntityId = origin.fromEntityId, toEntityId = selectedAirport.ApiKey, departDate = tomorrow, returnDate = nextWeek };

                List<Trip> itineraries = await GetRoundtripAsync(data);

                List<Trip> foundItineraries = itineraries
                    .Where(trip => trip.Flights
                    .SelectMany(flight => flight.Segments)
                    .All(segment => segment.CarrierId == carrierId))
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

        /// <summary>
        /// Method to get popular destinination async
        /// </summary>
        /// <returns> A list of destinations</returns>
        public async Task<List<City>> GetPopularDestinationsAsync()
        {
            List<City> destinationList = await _context.City.OrderByDescending(e => e.ApiKey).Take(5).ToListAsync();
            return destinationList;
        }

        //public async Task<List<Trip> GetThreeCheapestTripPrices()
        //{
        //    // Assumindo que você tem um DbSet no contexto chamado "Flight" com uma propriedade "Price"
        //    List<Trip> cheapestPrices = await _context.Trip.OrderBy(f => f.Price).Take(3).Select(f => f.Price).ToListAsync();

        //    return cheapestPrices;
        //}

    private string ConvertMinutesToTimeString(int durationInMinutes)
        {
            int hours = durationInMinutes / 60;
            int minutes = durationInMinutes % 60;
            return $"{hours:00}:{minutes:00}";
        }

        private Country SelectRandomCountry(List<Country> countries)
        {
            if (countries.Count == 1)
            {
                return countries[0];
            }
            var randomIndex = _random.Next(countries.Count);
            Country selectedCountry = countries[randomIndex];
            countries.RemoveAt(randomIndex);
            return selectedCountry;
        }

        //funciona para aeroportos e cidades
        private City SelectRandomAirport(List<City> cities)
        {
            if (cities.Count == 1)
            {
                return cities[0];
            }
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