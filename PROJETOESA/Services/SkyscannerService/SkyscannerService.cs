using Azure;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Newtonsoft.Json.Linq;
using Polly;
using Polly.Timeout;
using PROJETOESA.Data;
using PROJETOESA.Models;
using PROJETOESA.Models.ViewModels;
using System.Collections.Generic;
using System.Diagnostics;

namespace PROJETOESA.Services.SkyscannerService
{
    public class SkyscannerService : ISkyscannerService
    {
        private readonly HttpClient _httpClient;
        //private readonly AeroHelperContext _context;
        private readonly IDbContextFactory<AeroHelperContext> _contextFactory;
        private readonly Random _random = new Random();

        public SkyscannerService(IHttpClientFactory httpClientFactory, IDbContextFactory<AeroHelperContext> contextFactory)
        {
            _httpClient = httpClientFactory.CreateClient("SkyscannerAPI");
            _contextFactory = contextFactory;
        }

        /// <summary>
        /// Este método retorna todas as viagens de ida e volta.
        /// </summary>
        /// <param name="data">O Código da cidade de origem e destino, a data de partida e de chegada, os restantes dados são opcionais</param>
        /// <returns>Lista de algumas viagens disponíveis, para fornecer uma lista completa é necessário fazer a chamada ao método GetCompleteData</returns>
        public async Task<List<TripViewModel>> GetRoundtripAsync(FlightData data)
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

            bool requestSucceeded = false;
            HttpResponseMessage response = null;
            var retryPolicy = Policy
                .Handle<HttpRequestException>()
                .OrResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
                .WaitAndRetryAsync(3, attempt => TimeSpan.FromSeconds(2),
                    onRetry: (outcome, timespan, retryAttempt, context) =>
                    {
                        Console.WriteLine($"Tentativa {retryAttempt}: A chamada à API falhou. Tentando novamente em {timespan.TotalSeconds} segundos.");
                    });

            try
            {
                response = await retryPolicy.ExecuteAsync(async () =>
                {
                    var httpResponse = await _httpClient.GetAsync($"flights/search-roundtrip?{queryString}");
                    if (!httpResponse.IsSuccessStatusCode)
                    {
                        Console.WriteLine($"Resposta da API não foi bem-sucedida: {await httpResponse.Content.ReadAsStringAsync()}");
                    }
                    return httpResponse;
                });

                if (response.IsSuccessStatusCode)
                {
                    requestSucceeded = true;
                }
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Erro ao realizar a solicitação: {ex.Message}");
            }

            if (!requestSucceeded)
            {
                Console.WriteLine("A solicitação HTTP falhou após retentativas ou a resposta é nula. Continuando com o próximo processo...");
                return new List<TripViewModel>();
            }

            var content = await response.Content.ReadAsStringAsync();

            // Organizar os dados
            var jsonObject = JObject.Parse(content);
            var itinerariesData = jsonObject["data"]["itineraries"] as JArray;

            var sessionID = jsonObject["data"]["flightsSessionId"].ToString();
            var token = jsonObject["data"]["token"].ToString();

            List<TripViewModel> trips = new List<TripViewModel>();

            if (itinerariesData != null)
            {
                foreach (var itinerary in itinerariesData)
                {
                    TripViewModel trip = new TripViewModel
                    {
                        Id = itinerary["id"]?.ToString(),
                        Price = (double?)itinerary["price"]?["raw"] ?? 0.0,
                        SessionId = sessionID,
                        Token = token,
                        Flights = new List<FlightViewModel>()
                    };

                    int position = 0;
                    var legs = itinerary["legs"] as JArray;
                    if (legs != null)
                    {
                        foreach (var leg in legs)
                        {
                            City originCity = await GetCityAsync(leg["origin"]["id"]?.ToString(), leg["origin"]["name"]?.ToString(), leg["origin"]["country"]?.ToString());
                            City destinationCity = await GetCityAsync(leg["destination"]["id"]?.ToString(), leg["destination"]["name"]?.ToString(), leg["destination"]["country"]?.ToString());

                            var flight = new FlightViewModel
                            {
                                Id = leg["id"]?.ToString(),
                                Duration = ConvertMinutesToTimeString(leg["durationInMinutes"].ToObject<int>()),
                                Departure = DateTime.Parse(leg["departure"]?.ToString()),
                                Arrival = DateTime.Parse(leg["arrival"]?.ToString()),
                                OriginCity = new CityViewModel
                                {
                                    Id = originCity.Id,
                                    Name = originCity.Name,
                                    Coordenates = originCity.Coordenates,
                                    ApiKey = originCity.ApiKey,
                                    Country = new Country
                                    {
                                        Id = originCity.Country.Id,
                                        Name = originCity.Country.Name
                                    }
                                },
                                DestinationCity = new CityViewModel
                                {
                                    Id = destinationCity.Id,
                                    Name = destinationCity.Name,
                                    Coordenates = destinationCity.Coordenates,
                                    ApiKey = destinationCity.ApiKey,
                                    Country = new Country
                                    {
                                        Id = destinationCity.Country.Id,
                                        Name = destinationCity.Country.Name
                                    }
                                },
                                Direction = position == 0 ? "Departure" : "Arrival",
                                Segments = new List<SegmentViewModel>()
                            };

                            position++;

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

                                    var item = new SegmentViewModel
                                    {
                                        FlightNumber = segment["flightNumber"]?.ToString(),
                                        Departure = DateTime.Parse(segment["departure"]?.ToString()),
                                        Arrival = DateTime.Parse(segment["arrival"]?.ToString()),
                                        Duration = ConvertMinutesToTimeString(segment["durationInMinutes"].ToObject<int>()),
                                        OriginCity = new CityViewModel
                                        {
                                            Id = originCitySegment.Id,
                                            Name = originCitySegment.Name,
                                            Coordenates = originCitySegment.Coordenates,
                                            ApiKey = originCitySegment.ApiKey,
                                            Country = new Country
                                            {
                                                Id = originCitySegment.Country.Id,
                                                Name = originCitySegment.Country.Name
                                            }
                                        },
                                        DestinationCity = new CityViewModel
                                        {
                                            Id = destinationCitySegment.Id,
                                            Name = destinationCitySegment.Name,
                                            Coordenates = destinationCitySegment.Coordenates,
                                            ApiKey = destinationCitySegment.ApiKey,
                                            Country = new Country
                                            {
                                                Id = destinationCitySegment.Country.Id,
                                                Name = destinationCitySegment.Country.Name
                                            }
                                        },
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

        /// <summary>
        /// Este método chama GetRoundtripAsync para recolher as viagens disponíveis e depois junta tudo em diversas tarefas, 
        /// depois é feito várias chamadas a GetTripDetailsAsync ao mesmo tempo para recolher as informações completas sobre cada viagem, 
        /// por fim junta tudo e envia os resultados.
        /// Ao conseguir fazer todas as chamadas à API ao mesmo tempo, o tempo de espera pela resposta é muito inferior.
        /// </summary>
        /// <param name="data">O Código da cidade de origen e destino, a data de partida e de chegada, os restantes dados são opcionais</param>
        /// <returns>Os detalhes completos de cada viagem recolhida em GetRoundtripAsync</returns>
        public async Task<List<TripViewModel>> GetRoundtripPremiumAsync(FlightData data)
        {
            List<TripViewModel> trips = await GetRoundtripAsync(data);

            var tasks = new List<Task<TripViewModel>>();

            foreach (TripViewModel trip in trips)
            {
                tasks.Add(GetTripDetailsAsync(trip.Token, trip.Id, trip.SessionId));
            }

            var results = await Task.WhenAll(tasks);

            var tripDetails = results.Where(detail => detail != null).ToList();

            return tripDetails;
        }

        /// <summary>
        /// Este método é responsável por recolher toda a informação que a API Skyscanner contém sobre a viagem especifica
        /// </summary>
        /// <param name="token">O token que é disponibilizado pela API SkyScanner que atribui à chamada, é armazenado em Trip.Token</param>
        /// <param name="itineraryId">O Id de Trip que é composto pelo Id dos Voos</param>
        /// <param name="sessionId">A sessão que é atribuída pela API SkyScanner quando se faz uma chamada, é armazenada em Trip.SessionId</param>
        /// <returns>A informação completa do voo pesquisado, itineraryId</returns>
        public async Task<TripViewModel> GetTripDetailsAsync(string token, string itineraryId, string sessionId)
        {
            bool requestSucceeded = false;
            HttpResponseMessage response = null;
            var retryPolicy = Policy
                .Handle<HttpRequestException>()
                .OrResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
                .WaitAndRetryAsync(3, attempt => TimeSpan.FromSeconds(2),
                    onRetry: (outcome, timespan, retryAttempt, context) =>
                    {
                        Console.WriteLine($"Tentativa {retryAttempt}: A chamada à API falhou. Tentando novamente em {timespan.TotalSeconds} segundos.");
                    });

            try
            {
                response = await retryPolicy.ExecuteAsync(async () =>
                {
                    var httpResponse = await _httpClient.GetAsync($"/flights/detail?token={token}&itineraryId={itineraryId}");
                    if (!httpResponse.IsSuccessStatusCode)
                    {
                        Console.WriteLine($"Resposta da API não foi bem-sucedida: {await httpResponse.Content.ReadAsStringAsync()}");
                    }
                    return httpResponse;
                });

                if (response.IsSuccessStatusCode)
                {
                    requestSucceeded = true;
                }
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Erro ao realizar a solicitação: {ex.Message}");
            }

            if (!requestSucceeded)
            {
                Console.WriteLine("A solicitação HTTP falhou após retentativas ou a resposta é nula. Continuando com o próximo processo...");
                return new TripViewModel();
            }

            var content = await response.Content.ReadAsStringAsync();

            // Organizar os dados
            var jsonObject = JObject.Parse(content);
            var itineraryData = jsonObject["data"]["itinerary"];

            TripViewModel model = new TripViewModel();

            if (itineraryData != null)
            {
                model = new TripViewModel
                {
                    Id = itineraryData["id"].ToString(),
                    Token = token,
                    SessionId = sessionId,
                    Flights = new List<FlightViewModel>(),
                    PriceOptions = new List<PriceOptions>()
                };

                int position = 0;
                var legs = itineraryData["legs"] as JArray;
                var priceOptions = itineraryData["pricingOptions"] as JArray;

                if (legs != null)
                {
                    foreach (var leg in legs)
                    {
                        City originCity = await GetCityAsync(leg["origin"]["displayCode"]?.ToString());
                        City destinationCity = await GetCityAsync(leg["destination"]["displayCode"]?.ToString());

                        var flight = new FlightViewModel
                        {
                            Id = leg["id"]?.ToString(),
                            Departure = DateTime.Parse(leg["departure"]?.ToString()),
                            Arrival = DateTime.Parse(leg["arrival"]?.ToString()),
                            Duration = ConvertMinutesToTimeString(leg["duration"].ToObject<int>()),
                            Direction = position == 0 ? "Departure" : "Arrival",
                            OriginCity = new CityViewModel
                            {
                                Id = originCity.Id,
                                Name = originCity.Name,
                                Coordenates = originCity.Coordenates,
                                ApiKey = originCity.ApiKey,
                                Country = new Country
                                {
                                    Id = originCity.Country.Id,
                                    Name = originCity.Country.Name
                                }
                            },
                            DestinationCity = new CityViewModel
                            {
                                Id = destinationCity.Id,
                                Name = destinationCity.Name,
                                Coordenates = destinationCity.Coordenates,
                                ApiKey = destinationCity.ApiKey,
                                Country = new Country
                                {
                                    Id = destinationCity.Country.Id,
                                    Name = destinationCity.Country.Name
                                }
                            },
                            Segments = new List<SegmentViewModel>()
                        };

                        position++;

                        var segments = leg["segments"] as JArray;
                        if (segments != null)
                        {
                            foreach (var segment in segments)
                            {
                                City originCitySegment = await GetCityAsync(segment["origin"]["displayCode"]?.ToString());
                                City destinationCitySegment = await GetCityAsync(segment["destination"]["displayCode"]?.ToString());
                                Carrier carrierSegment = await GetCarrierAsync(segment["marketingCarrier"]["id"]?.ToString());

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
                                        Coordenates = originCitySegment.Coordenates,
                                        ApiKey = originCitySegment.ApiKey,
                                        Country = new Country
                                        {
                                            Id = originCitySegment.Country.Id,
                                            Name = originCitySegment.Country.Name
                                        }
                                    },
                                    DestinationCity = new CityViewModel
                                    {
                                        Id = destinationCitySegment.Id,
                                        Name = destinationCitySegment.Name,
                                        Coordenates = destinationCitySegment.Coordenates,
                                        ApiKey = destinationCitySegment.ApiKey,
                                        Country = new Country
                                        {
                                            Id = destinationCitySegment.Country.Id,
                                            Name = destinationCitySegment.Country.Name
                                        }
                                    },
                                    Carrier = carrierSegment
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
            return model;
        }

        /// <summary>
        /// Este método retorna uma lista de todos os possíveis destinos a partir de uma certa localidade
        /// </summary>
        /// <param name="data">É apenas necessário a Key do pais de origem</param>
        /// <returns>Retorna uma lista de Países que é possível viajar</returns>
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
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                // Aqui você poderia também logar o erro ou tomar outras ações necessárias.
                throw new HttpRequestException($"Response from the API was not successful: {errorContent}");
            }

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
                    using (var _context = _contextFactory.CreateDbContext())
                    {
                        Country country = await _context.Country.FirstOrDefaultAsync(c => c.Id == codeLocation);

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
            }

            return countriesDict.Values.ToList();
        }

        // Falta depois Organizar como se recebe os dados

        /// <summary>
        /// Este método é semelhante a GetRoundtripAsync, a diferença é que pesquisa viagem apenas de ida
        /// </summary>
        /// <param name="data">É necessário a key do país de origem e destino e a data de partida, os restantes dados são opcionais</param>
        /// <returns>Uma lista de viagens disponíveis</returns>
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

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Response from the API was not successful: {errorContent}");
            }

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

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Response from the API was not successful: {errorContent}");
            }

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

        /// <summary>
        /// Este método serve para poder retornar informação extra, por exemplo para saber a ApiKey de um determinado local,
        /// Uma lista de Aeroportos que existem num certo país e etc
        /// </summary>
        /// <param name="data">O nome da localidade ou país</param>
        /// <returns>Diferencia dependendo do que se passa como argumento</returns>
        public async Task<List<CustomGetDataModel>> GetDataAsync(string data)
        {
            HttpResponseMessage response = null;
            var retryPolicy = Policy
                .Handle<HttpRequestException>()
                .Or<TimeoutRejectedException>() // Caso um timeout seja acionado
                .OrResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
                .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    onRetry: (outcome, timespan, retryAttempt, context) =>
                    {
                        Console.WriteLine($"Tentativa {retryAttempt}: Falha ao chamar a API. Tentando novamente em {timespan.TotalSeconds} segundos.");
                    });

            var timeoutPolicy = Policy.TimeoutAsync<HttpResponseMessage>(10); // Timeout de 10 segundos

            try
            {
                response = await retryPolicy.ExecuteAsync(() =>
                    timeoutPolicy.ExecuteAsync(async () =>
                        await _httpClient.GetAsync($"flights/auto-complete?query={data}")
                    )
                );

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Response from the API was not successful: {errorContent}");
                    return new List<CustomGetDataModel>(); // Ou lidar com o erro de outra maneira.
                }
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"HttpRequestException: {ex.Message}");
                return new List<CustomGetDataModel>(); // Ou lidar com o erro de outra maneira.
            }
            catch (TimeoutRejectedException ex)
            {
                Console.WriteLine($"TimeoutRejectedException: A chamada à API excedeu o tempo limite. {ex.Message}");
                return new List<CustomGetDataModel>(); // Ou lidar com o erro de outra maneira.
            }

            // Processa a resposta bem-sucedida aqui
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

        /// <summary>
        /// Este método pesquisa na Base de dados todas as cidades existentes (Que têm aeroporto) num determinado País,
        /// se não houver nenhum país com esse nome na BD chama o método GetDataAsync para obter os dados e armazena na BD
        /// </summary>
        /// <param name="country">O país a pesquisar as cidades</param>
        /// <returns>Lista de cidades (Que têm aeroporto) desse pais</returns>
        public async Task<List<City>> GetAirportListAsync(Country country)
        {
            using (var _context = _contextFactory.CreateDbContext())
            {
                List<City> countries = await _context.City.Where(c => c.CountryId == country.Id).ToListAsync();

                if (countries.Count > 0)
                {
                    return countries;
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
        }

        /// <summary>
        /// Este método é apenas para poupar um pouco nas chamadas à API, simula uma chamada a GetSugestionsCompanyAsync
        /// </summary>
        /// <returns>Uma Trip de teste</returns>
        public async Task<List<TripViewModel>> GetSugestionsCompanyAsyncTest()
        {
            List<TripViewModel> trips = new List<TripViewModel>();

            TripViewModel trip = new TripViewModel
            {
                Id = "13577-2402250945--31781-0-11469-2402251030|11469-2403051635--31781-0-13577-2403051725",
                Price = 84.99,
                Flights = new List<FlightViewModel>()
            };

            City originCity = await GetCityAsync("LIS");
            City destinationCity = await GetCityAsync("FAO");

            var flight1 = new FlightViewModel
            {
                Id = "13577-2402250945--31781-0-11469-2402251030",
                Duration = "00:45",
                Departure = new DateTime(2024, 02, 25, 09, 45, 00),
                Arrival = new DateTime(2024, 02, 25, 10, 30, 00),
                OriginCity = new CityViewModel
                {
                    Id = originCity.Id,
                    Name = originCity.Name,
                    Coordenates = originCity.Coordenates,
                    ApiKey = originCity.ApiKey,
                    Country = new Country
                    {
                        Id = originCity.Country.Id,
                        Name = originCity.Country.Name
                    }
                },
                DestinationCity = new CityViewModel
                {
                    Id = destinationCity.Id,
                    Name = destinationCity.Name,
                    Coordenates = destinationCity.Coordenates,
                    ApiKey = destinationCity.ApiKey,
                    Country = new Country
                    {
                        Id = destinationCity.Country.Id,
                        Name = destinationCity.Country.Name
                    }
                },
                Direction = "Departure",
                Segments = new List<SegmentViewModel>()
            };

            var flight1Segment1 = new SegmentViewModel
            {
                FlightNumber = "1901",
                Departure = new DateTime(2024, 02, 25, 09, 45, 00),
                Arrival = new DateTime(2024, 02, 25, 10, 30, 00),
                Duration = "00:45",
                OriginCity = new CityViewModel
                {
                    Id = originCity.Id,
                    Name = originCity.Name,
                    Coordenates = originCity.Coordenates,
                    ApiKey = originCity.ApiKey,
                    Country = new Country
                    {
                        Id = originCity.Country.Id,
                        Name = originCity.Country.Name
                    }
                },
                DestinationCity = new CityViewModel
                {
                    Id = destinationCity.Id,
                    Name = destinationCity.Name,
                    Coordenates = destinationCity.Coordenates,
                    ApiKey = destinationCity.ApiKey,
                    Country = new Country
                    {
                        Id = destinationCity.Country.Id,
                        Name = destinationCity.Country.Name
                    }
                },
                Carrier = await GetCarrierAsync("-31781"),
            };

            flight1.Segments.Add(flight1Segment1);

            var flight2 = new FlightViewModel
            {
                Id = "11469-2403051635--31781-0-13577-2403051725",
                Duration = "00:50",
                Departure = new DateTime(2024, 03, 05, 16, 35, 00),
                Arrival = new DateTime(2024, 03, 05, 17, 25, 00),
                DestinationCity = new CityViewModel
                {
                    Id = originCity.Id,
                    Name = originCity.Name,
                    Coordenates = originCity.Coordenates,
                    ApiKey = originCity.ApiKey,
                    Country = new Country
                    {
                        Id = originCity.Country.Id,
                        Name = originCity.Country.Name
                    }
                },
                OriginCity = new CityViewModel
                {
                    Id = destinationCity.Id,
                    Name = destinationCity.Name,
                    Coordenates = destinationCity.Coordenates,
                    ApiKey = destinationCity.ApiKey,
                    Country = new Country
                    {
                        Id = destinationCity.Country.Id,
                        Name = destinationCity.Country.Name
                    }
                },
                Direction = "Arrival",
                Segments = new List<SegmentViewModel>()
            };

            var flight2Segment1 = new SegmentViewModel
            {
                FlightNumber = "1904",
                Duration = "00:50",
                Departure = new DateTime(2024, 03, 05, 16, 35, 00),
                Arrival = new DateTime(2024, 03, 05, 17, 25, 00),
                DestinationCity = new CityViewModel
                {
                    Id = originCity.Id,
                    Name = originCity.Name,
                    Coordenates = originCity.Coordenates,
                    ApiKey = originCity.ApiKey,
                    Country = new Country
                    {
                        Id = originCity.Country.Id,
                        Name = originCity.Country.Name
                    }
                },
                OriginCity = new CityViewModel
                {
                    Id = destinationCity.Id,
                    Name = destinationCity.Name,
                    Coordenates = destinationCity.Coordenates,
                    ApiKey = destinationCity.ApiKey,
                    Country = new Country
                    {
                        Id = destinationCity.Country.Id,
                        Name = destinationCity.Country.Name
                    }
                },
                Carrier = await GetCarrierAsync("-31781"),
            };

            flight2.Segments.Add(flight2Segment1);

            trip.Flights.Add(flight1);
            trip.Flights.Add(flight2);

            trips.Add(trip);

            return trips;
        }

        /// <summary>
        /// Este método serve para procurar uma lista de viagens que cujas têm apenas voos de uma determinada companhia aérea
        /// </summary>
        /// <param name="carrierId">Código da companhia aérea</param>
        /// <returns>Lista de Trip que têm apenas voos fornecidos de uma dada companhia aérea</returns>
        public async Task<List<TripViewModel>> GetSugestionsCompanyAsync(string carrierId)
        {
            FlightData origin = new FlightData { fromEntityId = "eyJlIjoiOTU1NjUwNTUiLCJzIjoiTElTIiwiaCI6IjI3NTQ0MDcyIn0=" };

            List<Country> possibleDestinations = await GetEverywhereAsync(origin);

            //Country countrySelected = SelectRandomCountry(possibleDestinations);

            var tasks = possibleDestinations.Select(destination => GetItinerariesAsync(destination, carrierId, origin.fromEntityId)).ToList();

            var results = await Task.WhenAll(tasks);

            List<TripViewModel> tripDetails = results.SelectMany(detail => detail).ToList();

            return tripDetails;
        }

        private async Task<List<TripViewModel>> GetItinerariesAsync(Country countrySelected, string carrierId, string originId)
        {
            string tomorrow = DateTime.Now.AddDays(1).ToString("yyyy-MM-dd");
            string nextWeek = DateTime.Now.AddDays(8).ToString("yyyy-MM-dd");

            using (var _context = _contextFactory.CreateDbContext())
            {
                List<City> cities = await _context.City.Where(c => c.CountryId == countrySelected.Id).ToListAsync();

                if (!cities.Any())
                {
                    cities = await GetAirportListAsync(countrySelected);
                }

                if(cities.Count > 0)
                {
                    City selectedAirport = SelectRandomAirport(cities);

                    FlightData data = new FlightData { fromEntityId = originId, toEntityId = selectedAirport.ApiKey, departDate = tomorrow, returnDate = nextWeek };

                    List<TripViewModel> itineraries = await GetRoundtripAsync(data);

                    List<TripViewModel> foundItineraries = itineraries
                        .Where(trip => trip.Flights
                            .SelectMany(flight => flight.Segments)
                            .All(segment => segment.Carrier != null && segment.Carrier.Id == carrierId))
                        .Take(3)
                        .ToList();

                    return foundItineraries;
                }
                return new List<TripViewModel>();
            }
        }

        private async Task<List<TripViewModel>> GetSortedTripsAsync(List<TripViewModel> unsortedTrips, List<string> sortedDestinationCityIds)
        {
            // Ordena as viagens com base na primeira ocorrência de seus destinos na lista sortedDestinationCityIds
            var sortedTrips = unsortedTrips.OrderBy(trip => {
                var firstFlightDestinationCityId = trip.Flights.FirstOrDefault()?.Segments.FirstOrDefault()?.DestinationCity.Id;
                var index = sortedDestinationCityIds.IndexOf(firstFlightDestinationCityId);
                // Se o destino não estiver na lista ordenada, coloca no final
                return index == -1 ? int.MaxValue : index;
            }).ToList();

            return sortedTrips;
        }

        /// <summary>
        /// Este método Pesquisa na base de dados todos os países que todos os utilizadores já viajaram
        /// </summary>
        /// <returns>Uma lista com as top 5 destinos ordenados pelo número de vezes que aparece na BD</returns>
        public async Task<List<City>> GetFavouriteDestinationsAsync()
        {
            var sortedCityIds = await GetSortedDestinationCityIdsAsync();

            // Pega os primeiros 5 IDs
            var topCityIds = sortedCityIds.Take(5);

            using (var _context = _contextFactory.CreateDbContext())
            {
                var topCities = await _context.City
                    .Where(c => topCityIds.Contains(c.Id))
                    .ToListAsync();

                return topCities;
            }
        }

        private async Task<List<string>> GetSortedDestinationCityIdsAsync()
        {
            var cityCounts = new Dictionary<string, int>();

            using (var _context = _contextFactory.CreateDbContext())
            {
                var trips = await _context.Trip
                    .Include(t => t.Flights)
                    .ThenInclude(f => f.DestinationCity)
                    .ToListAsync();

                foreach (var trip in trips)
                {
                    var relevantFlight = trip.Flights.Count == 1 ? trip.Flights.First() : trip.Flights.OrderBy(f => f.Departure).First();

                    if (cityCounts.ContainsKey(relevantFlight.DestinationCityId))
                    {
                        cityCounts[relevantFlight.DestinationCityId]++;
                    }
                    else
                    {
                        cityCounts[relevantFlight.DestinationCityId] = 1;
                    }
                }
            }

            var sortedCityIds = cityCounts.OrderByDescending(kv => kv.Value).Select(kv => kv.Key).ToList();
            return sortedCityIds;
        }

        /// <summary>
        /// Este método serve para poder pesquisar viagens consoante os destinos favoritos dos utilizadores
        /// </summary>
        /// <returns>Uma lista de viagens com destinos recolhidos em GetFavouriteDestinationsAsync</returns>
        public async Task<List<TripViewModel>> GetSugestionsDestinationsAsync()
        {
            string tomorrow = DateTime.Now.AddDays(1).ToString("yyyy-MM-dd");
            string nextWeek = DateTime.Now.AddDays(8).ToString("yyyy-MM-dd");
            FlightData origin = new FlightData { fromEntityId = "eyJlIjoiOTU1NjUwNTUiLCJzIjoiTElTIiwiaCI6IjI3NTQ0MDcyIn0=" };

            List<City> possibleDestinations = await GetFavouriteDestinationsAsync();

            List<TripViewModel> finalItineraries = new List<TripViewModel>();

            foreach (City currentCity in possibleDestinations)
            {
                FlightData data = new FlightData { fromEntityId = origin.fromEntityId, toEntityId = currentCity.ApiKey, departDate = tomorrow, returnDate = nextWeek };

                List<TripViewModel> itineraries = await GetRoundtripAsync(data);

                List<TripViewModel> foundItineraries = itineraries
                    .Take(2)
                    .ToList();

                finalItineraries.AddRange(foundItineraries);
            }

            return finalItineraries;
        }

        /// <summary>
        /// Este método Retorna as Top 5 companhias aéreas
        /// </summary>
        /// <returns>Uma lista de Top 5 Companhias aéreas</returns>
        public async Task<List<Carrier>> GetFavoriteAirlineAsync()
        {
            using (var _context = _contextFactory.CreateDbContext())
            {
                List<Carrier> carrierList = await _context.Carrier.OrderByDescending(e => e.SearchTimes).Take(5).ToListAsync();

                return carrierList;
            }
        }

        /// <summary>
        /// Este método serve para poder retornar a informação completa de uma cidade,
        /// A partir de cityId é possivel retornar toda a informação disponível na BD,
        /// Se essa cidade ainda não existir e se cityName e countryName não forem null, Armazena na BD a nova Cidade
        /// </summary>
        /// <param name="cityId">Código da cidade</param>
        /// <param name="cityName">Nome da cidade</param>
        /// <param name="countryName">Nome do país</param>
        /// <returns>A informação completa da cidade, incluindo o país em que pertence</returns>
        private async Task<City> GetCityAsync(string cityId, string? cityName = null, string? countryName = null)
        {
            using (var _context = _contextFactory.CreateDbContext())
            {
                City existingCity = await _context.City.Include(c => c.Country).FirstOrDefaultAsync(c => c.Id == cityId);

                Debug.WriteLine("Resultado da pesquisa de cidade");
                Debug.WriteLine(existingCity?.Name);

                Debug.WriteLine("Outros dados?");
                Debug.WriteLine(cityName);
                Debug.WriteLine(countryName);
                Debug.WriteLine(existingCity);

                if (existingCity != null)
                {
                    return existingCity;
                }

                var country = await _context.Country.FirstOrDefaultAsync(c => c.Name == countryName);
                if (country == null)
                {
                    Console.WriteLine($"País não encontrado: {countryName}");
                    return null;
                }

                Debug.WriteLine("O país foi encontrado");
                Debug.WriteLine(country.Name);

                List<CustomGetDataModel> data = await GetDataAsync(cityId);

                Debug.WriteLine("Pesquisa de data");
                Debug.WriteLine(data.Count);

                var item = data.FirstOrDefault(d => d.cityId == cityId);

                Debug.WriteLine("Dado encontrado");
                Debug.WriteLine(item?.city);

                if (item != null)
                {
                    Debug.WriteLine("item encontrado!!!!!!");
                    var newCity = new City { Id = cityId, Name = cityName, CountryId = country.Id, ApiKey = item.apiKey, Country = country };
                    try
                    {
                        _context.City.Add(newCity);
                        await _context.SaveChangesAsync();
                        Debug.WriteLine("Guardou o novo dado");
                        return newCity;
                    }
                    catch (DbUpdateException ex) when (ex.InnerException is SqlException sqlEx && (sqlEx.Number == 2627 || sqlEx.Number == 2601))
                    {
                        return await _context.City.Include(c => c.Country).FirstOrDefaultAsync(c => c.Id == cityId);
                    }
                }
                else
                {
                    Console.WriteLine($"Cidade não encontrada na API: {cityName}");
                    return null;
                }
            }
        }

        /// <summary>
        /// Este método serve para inserir na BD a informação de todas as companhias aéreas obtidas
        /// </summary>
        /// <param name="marketingCarriers">Lista de carriers obtidas pela pesquisa de viagens</param>
        /// <returns></returns>
        private async Task PopulateCarrierAsync(JArray marketingCarriers)
        {
            using (var _context = _contextFactory.CreateDbContext())
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

                        using var transaction = await _context.Database.BeginTransactionAsync();
                        try
                        {
                            await _context.Carrier.AddAsync(newCarrier);
                            await _context.SaveChangesAsync();
                            await transaction.CommitAsync();
                        }
                        catch (Exception)
                        {
                            await transaction.RollbackAsync();
                            throw;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Este método serve para poder retornar a informação completa de uma companhia aérea
        /// </summary>
        /// <param name="carrierId">O código de carrier</param>
        /// <returns>A informação completa existente na BD sobre essa carrier</returns>
        private async Task<Carrier> GetCarrierAsync(string carrierId)
        {
            using (var _context = _contextFactory.CreateDbContext())
            {
                return await _context.Carrier.FirstOrDefaultAsync(c => c.Id == carrierId);
            }
        }

        /// <summary>
        /// Converte um int em string 70 = 01:10
        /// </summary>
        /// <param name="durationInMinutes">Número inteiro em minutos</param>
        /// <returns>string convertida em horas hh:mm</returns>
        private string ConvertMinutesToTimeString(int durationInMinutes)
        {
            int hours = durationInMinutes / 60;
            int minutes = durationInMinutes % 60;
            return $"{hours:00}:{minutes:00}";
        }

        /// <summary>
        /// Seleciona um país aleatório da lista fornecida e retira esse país da lista para que evite possibilidade iguais
        /// </summary>
        /// <param name="countries">Lista de países</param>
        /// <returns>País escolhido aleatóriamente</returns>
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

        /// <summary>
        /// Seleciona uma cidade aleatória
        /// </summary>
        /// <param name="cities">Lista de cidades</param>
        /// <returns>Cidade aleatória</returns>
        private City SelectRandomAirport(List<City> cities)
        {
            if (!cities.Any())
            {
                throw new InvalidOperationException("Não é possível selecionar um aeroporto de uma lista vazia.");
            }

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