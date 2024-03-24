
using Mailjet.Client.Resources;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using PROJETOESA.Controllers;
using PROJETOESA.Data;
using PROJETOESA.Models;
using PROJETOESA.Models.ViewModels;
using Square.Models;
using System.Diagnostics;
using System.Net.Http;
using static Org.BouncyCastle.Crypto.Engines.SM2Engine;

namespace PROJETOESA.Services.SkyscannerService
{
    public class SkyscannerService : ISkyscannerService
    {
        private readonly HttpClient _httpClient;
        private readonly AeroHelperContext _context;
        private readonly Random _random = new Random();

        public SkyscannerService(IHttpClientFactory httpClientFactory, AeroHelperContext context)
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

            var sessionID = jsonObject["data"]["flightsSessionId"].ToString();
            var token = jsonObject["data"]["token"].ToString();

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
                        SessionId = sessionID,
                        Token = token,
                        Flights = new List<Flight>()
                    };

                    var legs = itinerary["legs"] as JArray;
                    if (legs != null)
                    {
                        foreach (var leg in legs)
                        {
                            City originCity = await GetCityAsync(leg["origin"]["id"]?.ToString(), leg["origin"]["name"]?.ToString(), leg["origin"]["country"]?.ToString());
                            City destinationCity = await GetCityAsync(leg["destination"]["id"]?.ToString(), leg["destination"]["name"]?.ToString(), leg["destination"]["country"]?.ToString());

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
                                await PopulateCarrierAsync(marketingCarriers);
                            }

                            var segments = leg["segments"] as JArray;
                            if (segments != null)
                            {
                                foreach (var segment in segments)
                                {
                                    City originCitySegment = await GetCityAsync(segment["origin"]["flightPlaceId"]?.ToString(), segment["origin"]["parent"]["name"]?.ToString(), segment["origin"]["country"]?.ToString());
                                    City destinationCitySegment = await GetCityAsync(segment["destination"]["flightPlaceId"]?.ToString(), segment["destination"]["parent"]["name"]?.ToString(), segment["destination"]["country"]?.ToString());
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

        public async Task<List<TripDetailsViewModel>> GetRoundtripPremiumAsync(FlightData data)
        {
            List<Trip> trips = await GetRoundtripAsync(data);

            Debug.WriteLine("alasjdçaojisºdfoajsdfgºoaidjfgçºzlkdmfºvbgaopidfjgçºaoidfjhhg+ºoahjiertfgoiaehjtgoihjaetroghjaeotijhgaoetihjgajhertgjaodpkfmgº-aPojidkerºtpgjaºerpjgºae+9irjgõeparkjgkeoprg~pajkeri~gp9okedrpºalasjdçaojisºdfoajsdfgºoaidjfgçºzlkdmfºvbgaopidfjgçºaoidfjhhg+ºoahjiertfgoiaehjtgoihjaetroghjaeotijhgaoetihjgajhertgjaodpkfmgº-aPojidkerºtpgjaºerpjgºae+9irjgõeparkjgkeoprg~pajkeri~gp9okedrpº");
            Debug.WriteLine(trips.Count);
            // Inicializa uma lista de Task para cada detalhe de viagem que será obtido
            var tasks = new List<Task<TripDetailsViewModel>>();

            foreach (Trip trip in trips)
            {
                // Inicia a task sem esperar por ela aqui, adicionando-a à lista de tasks
                tasks.Add(GetTripDetailsAsync(trip.Token, trip.Id));
            }

            // Aguarda a conclusão de todas as tasks de detalhes de viagens simultaneamente
            var results = await Task.WhenAll(tasks);

            // Filtra os resultados para excluir os nulls e depois converte para lista
            var tripDetails = results.Where(detail => detail != null).ToList();

            Debug.WriteLine("alasjdçaojisºdfoajsdfgºoaidjfgçºzlkdmfºvbgaopidfjgçºaoidfjhhg+ºoahjiertfgoiaehjtgoihjaetroghjaeotijhgaoetihjgajhertgjaodpkfmgº-aPojidkerºtpgjaºerpjgºae+9irjgõeparkjgkeoprg~pajkeri~gp9okedrpºalasjdçaojisºdfoajsdfgºoaidjfgçºzlkdmfºvbgaopidfjgçºaoidfjhhg+ºoahjiertfgoiaehjtgoihjaetroghjaeotijhgaoetihjgajhertgjaodpkfmgº-aPojidkerºtpgjaºerpjgºae+9irjgõeparkjgkeoprg~pajkeri~gp9okedrpº");
            Debug.WriteLine(tripDetails.Count);

            // Retorna a lista filtrada
            return tripDetails;
        }


        public async Task<TripDetailsViewModel> GetTripDetailsAsync(string token, string itineraryId)
        {
            //var response = await _httpClient.GetAsync($"/flights/detail?token={token}&itineraryId={itineraryId}");

            HttpResponseMessage response = await _httpClient.GetAsync($"/flights/detail?token={token}&itineraryId={itineraryId}");

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();

            // Organizar os dados
            var jsonObject = JObject.Parse(content);
            var itineraryData = jsonObject["data"]["itinerary"];

            TripDetailsViewModel model = new TripDetailsViewModel();

            if (itineraryData != null)
            {
                //Debug.WriteLine(itineraryData["id"].ToString());
                //Debug.WriteLine(itineraryData["destinationImage"].ToString());

                model = new TripDetailsViewModel
                {
                    Id = itineraryData["id"].ToString(),
                    DestinationImage = itineraryData["destinationImage"].ToString(),
                    Flights = new List<FlightViewModel>(),
                    PriceOptions = new List<PriceOptions>()
                };

                var legs = itineraryData["legs"] as JArray;
                var priceOptions = itineraryData["pricingOptions"] as JArray;

                if (legs != null)
                {
                    foreach (var leg in legs)
                    {
                        //Debug.WriteLine("asdhasdohaoiudaiohdiauhdiuahdihdiuahdiuhaidhadhaidhaidhaidhaiuhdaihdishdiaushdiahdiuahdihdiaasdhasdohaoiudaiohdiauhdiuahdihdiuahdiuhaidhadhaidhaidhaidhaiuhdaihdishdiaushdiahdiuahdihdiaasdhasdohaoiudaiohdiauhdiuahdihdiuahdiuhaidhadhaidhaidhaidhaiuhdaihdishdiaushdiahdiuahdihdiaasdhasdohaoiudaiohdiauhdiuahdihdiuahdiuhaidhadhaidhaidhaidhaiuhdaihdishdiaushdiahdiuahdihdia");

                        //Debug.WriteLine("originDisplaycode");
                        //Debug.WriteLine(leg["origin"]["displayCode"]?.ToString());
                        //Debug.WriteLine("destination Display code");
                        //Debug.WriteLine(leg["destination"]["displayCode"]?.ToString());

                        City originCity = await GetCityAsync(leg["origin"]["displayCode"]?.ToString());
                        City destinationCity = await GetCityAsync(leg["destination"]["displayCode"]?.ToString());

                        //Debug.WriteLine("Depois da pesquisa, nome da origem");
                        //Debug.WriteLine(originCity.Name);
                        //Debug.WriteLine("Depois da pesquisa, nome do destino");
                        //Debug.WriteLine(destinationCity.Name);

                        //Debug.WriteLine("trip ID");
                        //Debug.WriteLine(leg["id"]?.ToString());
                        //Debug.WriteLine("Departure");
                        //Debug.WriteLine(leg["departure"]?.ToString());
                        //Debug.WriteLine("Arrival");
                        //Debug.WriteLine(leg["arrival"]?.ToString());
                        //Debug.WriteLine("duration");
                        //Debug.WriteLine(leg["duration"].ToObject<int>());
                        //Debug.WriteLine("stop count");
                        //Debug.WriteLine(leg["stopCount"]?.ToObject<int>());

                        var flight = new FlightViewModel
                        {
                            Id = leg["id"]?.ToString(),
                            Departure = DateTime.Parse(leg["departure"]?.ToString()),
                            Arrival = DateTime.Parse(leg["arrival"]?.ToString()),
                            Duration = ConvertMinutesToTimeString(leg["duration"].ToObject<int>()),
                            OriginCity = new CityViewModel
                            {
                                Id = originCity.Id,
                                Name = originCity.Name,
                                ApiKey = originCity.ApiKey,
                                Country = new CountryViewModel
                                {
                                    Id = originCity.Country.Id,
                                    Name = originCity.Country.Name
                                }
                            },
                            DestinationCity = new CityViewModel
                            {
                                Id = destinationCity.Id,
                                Name = destinationCity.Name,
                                ApiKey = destinationCity.ApiKey,
                                Country = new CountryViewModel
                                {
                                    Id = destinationCity.Country.Id,
                                    Name = destinationCity.Country.Name
                                }
                            },
                            StopCount = leg["stopCount"]?.ToObject<int>(),
                            Segments = new List<SegmentViewModel>()
                        };

                        var segments = leg["segments"] as JArray;
                        if (segments != null)
                        {
                            foreach (var segment in segments)
                            {
                                //Debug.WriteLine("Segmento");
                                //Debug.WriteLine("originDisplaycode");
                                //Debug.WriteLine(segment["origin"]["displayCode"]?.ToString());
                                //Debug.WriteLine("destination Display code");
                                //Debug.WriteLine(segment["destination"]["displayCode"]?.ToString());
                                //Debug.WriteLine("Carrier");
                                //Debug.WriteLine(segment["marketingCarrier"]["id"]?.ToString());

                                City originCitySegment = await GetCityAsync(segment["origin"]["displayCode"]?.ToString());
                                City destinationCitySegment = await GetCityAsync(segment["destination"]["displayCode"]?.ToString());
                                Carrier carrierSegment = await GetCarrierAsync(segment["marketingCarrier"]["id"]?.ToString());

                                //Debug.WriteLine("Depois da pesquisa Origem");
                                //Debug.WriteLine(originCitySegment.Name);
                                //Debug.WriteLine("Depois da pesquisa destino");
                                //Debug.WriteLine(destinationCitySegment.Name);
                                //Debug.WriteLine("Depois da pesquisa Carrier");
                                //Debug.WriteLine(carrierSegment.Name);

                                //Debug.WriteLine("Flight Number");
                                //Debug.WriteLine(segment["flightNumber"]?.ToString());
                                //Debug.WriteLine("Departure");
                                //Debug.WriteLine(segment["departure"]?.ToString());
                                //Debug.WriteLine("Arrival");
                                //Debug.WriteLine(segment["arrival"]?.ToString());
                                //Debug.WriteLine("Duration");
                                //Debug.WriteLine(segment["duration"].ToObject<int>()); 

                                var item = new SegmentViewModel
                                {
                                    FlightNumber = segment["flightNumber"]?.ToString(),
                                    Departure = DateTime.Parse(segment["departure"]?.ToString()),
                                    Arrival = DateTime.Parse(segment["arrival"]?.ToString()),
                                    Duration = ConvertMinutesToTimeString(segment["duration"].ToObject<int>()),
                                    OriginCity = new CityViewModel
                                    {
                                        Id = originCitySegment.Id,
                                        Name = originCitySegment.Name,
                                        ApiKey = originCitySegment.ApiKey,
                                        Country = new CountryViewModel
                                        {
                                            Id = originCitySegment.Country.Id,
                                            Name = originCitySegment.Country.Name
                                        }
                                    },
                                    DestinationCity = new CityViewModel
                                    {
                                        Id = destinationCitySegment.Id,
                                        Name = destinationCitySegment.Name,
                                        ApiKey = destinationCitySegment.ApiKey,
                                        Country = new CountryViewModel
                                        {
                                            Id = destinationCitySegment.Country.Id,
                                            Name = destinationCitySegment.Country.Name
                                        }
                                    },
                                    Carrier = new CarrierViewModel
                                    {
                                        Id = carrierSegment.Id,
                                        Name = carrierSegment.Name,
                                        LogoURL = carrierSegment.LogoURL,
                                        SearchTimes = carrierSegment.SearchTimes
                                    }
                                };
                                flight.Segments.Add(item);
                            }
                        }
                        model.Flights.Add(flight);
                    }
                }

                if (priceOptions != null)
                {
                    foreach (var price in priceOptions)
                    {
                        var agents = price["agents"] as JArray;
                        //Debug.WriteLine("ID");
                        //Debug.WriteLine(agents[0]["id"]?.ToString());
                        //Debug.WriteLine("Agente");
                        //Debug.WriteLine(agents[0]["name"]?.ToString());
                        //Debug.WriteLine("Position");
                        //Debug.WriteLine(agents[0]["bookingProposition"]?.ToString());
                        //Debug.WriteLine("URL");
                        //Debug.WriteLine(agents[0]["url"]?.ToString());
                        //Debug.WriteLine("Price");
                        //Debug.WriteLine((double)price["totalPrice"]);

                        var option = new PriceOptions
                        {
                            AgentId = agents[0]["id"]?.ToString(),
                            AgentName = agents[0]["name"]?.ToString(),
                            BookingPosition = agents[0]["bookingProposition"]?.ToString(),
                            OfferURL = agents[0]["url"]?.ToString(),
                            TotalPrice = (double)price["totalPrice"]
                        };

                        model.PriceOptions.Add(option);
                    }
                }
            }

            //Debug.WriteLine("asdhasdohaoiudaiohdiauhdiuahdihdiuahdiuhaidhadhaidhaidhaidhaiuhdaihdishdiaushdiahdiuahdihdiaasdhasdohaoiudaiohdiauhdiuahdihdiuahdiuhaidhadhaidhaidhaidhaiuhdaihdishdiaushdiahdiuahdihdiaasdhasdohaoiudaiohdiauhdiuahdihdiuahdiuhaidhadhaidhaidhaidhaiuhdaihdishdiaushdiahdiuahdihdiaasdhasdohaoiudaiohdiauhdiuahdihdiuahdiuhaidhadhaidhaidhaidhaiuhdaihdishdiaushdiahdiuahdihdia");
            //Debug.WriteLine(model.Id);


            return model;
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

        public async Task<List<CustomGetDataModel>> GetDataAsync(string data)
        {
            var response = await _httpClient.GetAsync($"flights/auto-complete?query={data}");
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
                        apiKey = item["presentation"]["id"]?.ToString(),
                        cityId = item["navigation"]["relevantFlightParams"]["skyId"]?.ToString(),
                        city = item["navigation"]["localizedName"]?.ToString(),
                        flightPlaceType = item["navigation"]["relevantFlightParams"]["flightPlaceType"]?.ToString(),
                        country = item["presentation"]["subtitle"]?.ToString(),
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
                var result = await GetDataAsync(country.Name);

                List<City> cities = new List<City>();

                foreach (var item in result)
                {
                    if (item.flightPlaceType != "COUNTRY")
                    {

                        var airport = new City
                        {
                            Id = item.cityId,
                            Name = item.city,
                            ApiKey = item.apiKey,
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

        public async Task<List<City>> GetFavouriteDestinationsAsync()
        {
            var cityCounts = new Dictionary<string, int>();

            var trips = await _context.Trip
                .Include(t => t.Flights)
                .ThenInclude(f => f.DestinationCity)
                .ToListAsync();

            foreach (var trip in trips)
            {
                Flight relevantFlight;

                if (trip.Flights.Count == 1)
                {
                    relevantFlight = trip.Flights.First();
                }
                else
                {
                    relevantFlight = trip.Flights.OrderBy(f => f.Departure).First();
                }

                if (cityCounts.ContainsKey(relevantFlight.DestinationCityId))
                {
                    cityCounts[relevantFlight.DestinationCityId]++;
                }
                else
                {
                    cityCounts[relevantFlight.DestinationCityId] = 1;
                }
            }

            var topCityIds = cityCounts.OrderByDescending(kv => kv.Value)
                .Take(5)
                .Select(kv => kv.Key);

            var topCities = await _context.City
                .Where(c => topCityIds.Contains(c.Id))
                .ToListAsync();

            return topCities;
        }

        public async Task<List<Trip>> GetSugestionsDestinationsAsync()
        {
            string tomorrow = DateTime.Now.AddDays(1).ToString("yyyy-MM-dd");
            string nextWeek = DateTime.Now.AddDays(8).ToString("yyyy-MM-dd");
            FlightData origin = new FlightData { fromEntityId = "eyJlIjoiOTU1NjUwNTUiLCJzIjoiTElTIiwiaCI6IjI3NTQ0MDcyIn0=" };

            List<City> possibleDestinations = await GetFavouriteDestinationsAsync();

            List<Trip> finalItineraries = new List<Trip>();

            foreach (City currentCity in possibleDestinations)
            {
                FlightData data = new FlightData { fromEntityId = origin.fromEntityId, toEntityId = currentCity.ApiKey, departDate = tomorrow, returnDate = nextWeek };

                List<Trip> itineraries = await GetRoundtripAsync(data);

                List<Trip> foundItineraries = itineraries
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

        private async Task<City> GetCityAsync(string cityId, string? cityName = null, string? countryName = null)
        {
            var result = await _context.City.Include(c => c.Country).FirstOrDefaultAsync(c => c.Id == cityId);

            if (result == null && countryName != null && cityId != null && cityName != null)
            {
                var country = await _context.Country.FirstOrDefaultAsync(c => c.Name == countryName);

                List<CustomGetDataModel> data = await GetDataAsync(cityName);

                foreach(var item in data)
                {
                    if(item.cityId == cityId)
                    {
                        using var transaction = await _context.Database.BeginTransactionAsync();
                        try
                        {
                            var newCity = new City { Id = item.cityId, Name = cityName, ApiKey = item.apiKey, CountryId = country.Id, Country = country };
                            await _context.City.AddAsync(newCity);
                            
                            await _context.SaveChangesAsync();
                            await transaction.CommitAsync();

                            return newCity;
                        }
                        catch (Exception)
                        {
                            await transaction.RollbackAsync();
                        }
                        
                    }
                }
            }
         
            return result!;
        }

        private async Task PopulateCarrierAsync(JArray marketingCarriers)
        {
            foreach (var carrier in marketingCarriers)
            {
                var carrierId = carrier["id"]?.ToString();

                if (!await _context.Carrier.AnyAsync(c => c.Id == carrierId))
                {
                    var newCarrier = new Carrier
                    {
                        Id = carrierId,
                        Name = carrier["name"]?.ToString(),
                        LogoURL = carrier["logoUrl"]?.ToString(),
                    };

                    await _context.Carrier.AddAsync(newCarrier);
                    await _context.SaveChangesAsync();
                }
            }
        }

        private async Task<Carrier> GetCarrierAsync(string carrierId)
        {
            return await _context.Carrier.FirstOrDefaultAsync(c => c.Id == carrierId);
        }

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
        public string apiKey { get; set; }
        public string cityId { get; set; }
        public string city { get; set; }
        public string flightPlaceType { get; set; }
        public string country { get; set; }
    }
}