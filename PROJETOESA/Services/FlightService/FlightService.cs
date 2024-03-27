using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using PROJETOESA.Controllers;
using PROJETOESA.Data;
using PROJETOESA.Models;
using PROJETOESA.Models.ViewModels;
using PROJETOESA.Services.SkyscannerService;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace PROJETOESA.Services.FlightService
{
    public class FlightService : IFlightService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey = "ef9543daa9ed9c4a8cb31fca069c6107";
        private readonly string _googleApiKey = "AIzaSyD58yFxevJ8McI8Wc1WxUfx9EhVl-6D4gQ";
        private readonly AeroHelperContext _context;
        private readonly ISkyscannerService _skyscannerService;

        public FlightService(HttpClient httpClient, AeroHelperContext context, ISkyscannerService skyscannerService)
        {
            _httpClient = httpClient;
            _context = context;
            _skyscannerService = skyscannerService;
        }

        public async Task<List<FlightsItinerary>> GetLiveFlightData()
        {
            var url = $"http://api.aviationstack.com/v1/flights?access_key={_apiKey}";
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();

            var jsonObject = JObject.Parse(content);
            var flightsData = jsonObject["data"] as JArray;

            List<FlightsItinerary> itineraries = new List<FlightsItinerary>();

            if (flightsData != null)
            {
                foreach (var flight in flightsData)
                {
                    City departureCity = await GetCityAsync(flight["departure"]["timezone"]?.ToString(), flight["departure"]["airport"]?.ToString());
                    City arrivalCity = await GetCityAsync(flight["arrival"]["timezone"]?.ToString(), flight["arrival"]["airport"]?.ToString());
                    Carrier carrier = await GetCarrierAsync(flight["airline"]["name"]?.ToString());

                    var itinerary = new FlightsItinerary
                    {
                        FlightIATA = flight["flight"]["iata"]?.ToString(),
                        FlightICAO = flight["flight"]["icao"]?.ToString(),
                        FlightStatus = flight["flight_status"]?.ToString(),
                        DepartureLocation = departureCity,
                        DepartureSchedule = DateTime.Parse(flight["departure"]["scheduled"]?.ToString()),
                        ArrivalLocation = arrivalCity,
                        ArrivalSchedule = DateTime.Parse(flight["arrival"]["scheduled"]?.ToString()),
                        Airline = carrier
                    };

                    itineraries.Add(itinerary);
                }
            }

            return itineraries;
        }

        // Metodo que lê um ficheiro Json para que não tenha de fazer chamada à API
        public async Task<List<FlightsItinerary>> LoadFlightsAsync()
        {
            string filePath = "./Assets/dados.json";

            using (StreamReader file = File.OpenText(filePath))
            {
                var content = await file.ReadToEndAsync();
                var jsonArray = JArray.Parse(content);

                List<FlightsItinerary> itineraries = new List<FlightsItinerary>();

                foreach (var flightObject in jsonArray)
                {
                    var itinerary = new FlightsItinerary
                    {
                        FlightIATA = flightObject["flightIATA"]?.ToString(),
                        FlightICAO = flightObject["flightICAO"]?.ToString(),
                        FlightStatus = flightObject["flightStatus"]?.ToString(),
                        DepartureLocation = flightObject["departureLocation"].ToObject<City>(),
                        DepartureSchedule = (DateTime)flightObject["departureSchedule"],
                        ArrivalLocation = flightObject["arrivalLocation"].ToObject<City>(),
                        ArrivalSchedule = (DateTime)flightObject["arrivalSchedule"],
                        Airline = flightObject["airline"].ToObject<Carrier>()
                    };

                    itineraries.Add(itinerary);
                }

                return itineraries;
            }
        }

        public async Task<(string latitude, string longitude)> GetCoordenatesAsync(string locationName)
        {
            var city = await _context.City.FirstOrDefaultAsync(c => c.Name == locationName);

            string mainLatitude = "";
            string mainLongitude = "";

            if (city != null && !string.IsNullOrEmpty(city.Coordinates))
            {
                var parts = city.Coordinates.Split(';');
                if (parts.Length == 2)
                {
                    mainLatitude = parts[0];
                    mainLongitude = parts[1];
                }
            }
            else
            {
                string geocodeUrl = $"https://maps.googleapis.com/maps/api/geocode/json?address={Uri.EscapeDataString(locationName)}%airport&key={_googleApiKey}";

                using (HttpClient client = new HttpClient())
                {
                    var response = await client.GetAsync(geocodeUrl);
                    var content = await response.Content.ReadAsStringAsync();
                    var json = JObject.Parse(content);

                    var results = json["results"] as JArray;
                    if (results != null && results.Count > 0)
                    {
                        var location = results[0]["geometry"]["location"];
                        mainLatitude = location["lat"].ToString().Replace(",", ".");
                        mainLongitude = location["lng"].ToString().Replace(",", ".");

                        if (city != null)
                        {
                            city.Coordinates = $"{mainLatitude};{mainLongitude}";
                            _context.SaveChanges();
                        }
                    }
                }
            }
            return (mainLatitude, mainLongitude);
        }

        public async Task<string> GetAirlinesDataAsync()
        {

            var url = $"http://api.aviationstack.com/v1/airlines?access_key={_apiKey}";
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();

            return content;
        }

        //public async Task<List<Trip>> GetFlightsAsync(AddressComponents origin, AddressComponents destination)
        //{
        //    var allCities = await _context.City.Include(c => c.Country).ToListAsync();

        //    City FindCity(AddressComponents addressComponents)
        //    {
        //        var city = allCities.FirstOrDefault(c => c.Name.ToLower().Equals(addressComponents.city.ToLower()));

        //        if (city != null)
        //        {
        //            return city;
        //        }

        //        var originLatitudeText = addressComponents.latitude.Replace(',', '.');
        //        var originLongitudeText = addressComponents.longitude.Replace(',', '.');

        //        double originLatitude;
        //        double originLongitude;

        //        double.TryParse(originLatitudeText, NumberStyles.Any, CultureInfo.InvariantCulture, out originLatitude);
        //        double.TryParse(originLongitudeText, NumberStyles.Any, CultureInfo.InvariantCulture, out originLongitude);

        //        return allCities
        //            .Select(c => new
        //            {
        //                City = c,
        //                Distance = HaversineDistance(originLatitude, originLongitude, c)
        //            })
        //            .OrderBy(c => c.Distance)
        //            .First().City;
        //    }

        //    City originCity = FindCity(origin);
        //    City destinationCity = FindCity(destination);

        //    string tomorrow = DateTime.Now.AddDays(1).ToString("yyyy-MM-dd");
        //    string nextWeek = DateTime.Now.AddDays(8).ToString("yyyy-MM-dd");

        //    List<Trip> tripList = await _skyscannerService.GetRoundtripAsync(new FlightData { fromEntityId = originCity.ApiKey, toEntityId = destinationCity.ApiKey, departDate = tomorrow, returnDate = nextWeek });

        //    return tripList;
        //}

        public async Task<List<Trip>> GetFlightsAsync(AddressComponents origin, AddressComponents destination)
        {
            var allCities = await _context.City.Include(c => c.Country).ToListAsync();

            var citiesWithEmptyCoordenates = await _context.City.Include(c => c.Country).Where(c => string.IsNullOrEmpty(c.Coordinates)).ToListAsync();

            if (citiesWithEmptyCoordenates.Any())
            {
                allCities = await PopulateCoordinatesAsync();
            }

            City FindCity(AddressComponents addressComponents)
            {
                var city = allCities.FirstOrDefault(c => c.Name.ToLower().Equals(addressComponents.city.ToLower()));

                if (city != null)
                {
                    return city;
                }

                var originLatitudeText = addressComponents.latitude.Replace(',', '.');
                var originLongitudeText = addressComponents.longitude.Replace(',', '.');

                double originLatitude;
                double originLongitude;

                double.TryParse(originLatitudeText, NumberStyles.Any, CultureInfo.InvariantCulture, out originLatitude);
                double.TryParse(originLongitudeText, NumberStyles.Any, CultureInfo.InvariantCulture, out originLongitude);

                return allCities
                    .Select(c => new
                    {
                        City = c,
                        Distance = HaversineDistance(originLatitude, originLongitude, c)
                    })
                    .OrderBy(c => c.Distance)
                    .First().City;
            }

            City originCity = FindCity(origin);
            City destinationCity = FindCity(destination);

            string tomorrow = DateTime.Now.AddDays(1).ToString("yyyy-MM-dd");
            string nextWeek = DateTime.Now.AddDays(8).ToString("yyyy-MM-dd");

            List<Trip> tripList = await _skyscannerService.GetRoundtripAsync(new FlightData { fromEntityId = originCity.ApiKey, toEntityId = destinationCity.ApiKey, departDate = tomorrow, returnDate = nextWeek });

            return tripList;
        }

        public async Task<List<TripDetailsViewModel>> GetFlightsPremiumAsync(AddressComponents origin, AddressComponents destination)
        {
            List<Trip> trips = await GetFlightsAsync(origin, destination);

            var tasks = new List<Task<TripDetailsViewModel>>();

            foreach (Trip trip in trips)
            {
                tasks.Add(_skyscannerService.GetTripDetailsAsync(trip.Token, trip.Id));
            }

            var results = await Task.WhenAll(tasks);

            var tripDetails = results.Where(detail => detail != null).ToList();

            return tripDetails;
        }

        public async Task<List<City>> PopulateCoordinatesAsync()
        {
            var allCities = await _context.City.Include(c => c.Country).ToListAsync();
            foreach (var city in allCities)
            {
                if (string.IsNullOrEmpty(city.Coordinates))
                {
                    Debug.WriteLine("Cidade!!", city.Name);
                    string geocodeUrl = $"https://maps.googleapis.com/maps/api/geocode/json?address={Uri.EscapeDataString(city.Name)}&key={_googleApiKey}";

                    using (HttpClient client = new HttpClient())
                    {
                        var response = await client.GetAsync(geocodeUrl);
                        var content = await response.Content.ReadAsStringAsync();
                        var json = JObject.Parse(content);

                        var results = json["results"] as JArray;
                        if (results != null && results.Count > 0)
                        {
                            var location = results[0]["geometry"]["location"];
                            var mainLatitude = location["lat"].ToString().Replace(",", ".");
                            var mainLongitude = location["lng"].ToString().Replace(",", ".");

                            if (city != null)
                            {
                                city.Coordinates = $"{mainLatitude};{mainLongitude}";
                                Debug.WriteLine("Coordenadas!!", city.Coordinates);

                                _context.SaveChanges();
                            }
                        }
                    }
                }

            }

            return await _context.City.ToListAsync();
        }

        private double HaversineDistance(double lat1, double lon1, City city)
        {
            var coordinates = city.Coordinates.Split(';');
            double lat2;
            double lon2;

            var originLatitudeText = coordinates[0].Replace(',', '.');
            var originLongitudeText = coordinates[1].Replace(',', '.');

            double.TryParse(originLatitudeText, NumberStyles.Any, CultureInfo.InvariantCulture, out lat2);
            double.TryParse(originLongitudeText, NumberStyles.Any, CultureInfo.InvariantCulture, out lon2);

            double R = 6371;
            double dLat = ToRadians(lat2 - lat1);
            double dLon = ToRadians(lon2 - lon1);
            lat1 = ToRadians(lat1);
            lat2 = ToRadians(lat2);

            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) + Math.Sin(dLon / 2) * Math.Sin(dLon / 2) * Math.Cos(lat1) * Math.Cos(lat2);
            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return R * c;
        }

        private double ToRadians(double angle)
        {
            return Math.PI * angle / 180.0;
        }

        private async Task<Carrier> GetCarrierAsync(string airlineName)
        {
            var normalizedAirlineName = airlineName.ToLower().Trim();
            var allCarriers = await _context.Carrier.ToListAsync();

            Carrier mostSimilarCarrier = null;
            double highestSimilarity = 0.0;

            foreach (var carrier in allCarriers)
            {
                var normalizedCarrierName = carrier.Name.ToLower().Trim();
                var similarity = CalculateSimilarity(normalizedAirlineName, normalizedCarrierName);

                if (similarity > highestSimilarity)
                {
                    highestSimilarity = similarity;
                    mostSimilarCarrier = carrier;
                }
            }

            return mostSimilarCarrier;
        }

        private async Task<City> GetCityAsync(string timezone, string cityName)
        {
            var timezoneCityName = timezone.Split('/').Last().Replace("\\", "");

            var city = await _context.City.Include(c => c.Country).FirstOrDefaultAsync(c => EF.Functions.Like(c.Name, $"%{timezoneCityName}%"));

            if (city != null && string.IsNullOrEmpty(city.Coordinates))
            {
                await GetCoordenatesAsync(city.Name);
            }

            if (city != null)
            {
                return city;
            }

            return await GetMostSimilarCityAsync(cityName);
        }

        private async Task<City> GetMostSimilarCityAsync(string cityName)
        {
            var normalizedCityName = cityName.ToLower().Trim();
            var allCities = await _context.City.Include(c => c.Country).ToListAsync();

            City mostSimilarCity = null;
            double highestSimilarity = 0.0;

            foreach (var city in allCities)
            {
                var normalizedDbCityName = city.Name.ToLower().Trim();
                var similarity = CalculateSimilarity(normalizedCityName, normalizedDbCityName);

                if (similarity > highestSimilarity)
                {
                    highestSimilarity = similarity;
                    mostSimilarCity = city;
                }
            }

            if (string.IsNullOrEmpty(mostSimilarCity.Coordinates))
            {
                await GetCoordenatesAsync(mostSimilarCity.Name);
            }

            return mostSimilarCity;
        }

        private double CalculateSimilarity(string source, string target)
        {
            if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(target)) return 0.0;

            int commonCharacters = source.Intersect(target).Count();

            return (double)commonCharacters / Math.Max(source.Length, target.Length);
        }
    }
}
