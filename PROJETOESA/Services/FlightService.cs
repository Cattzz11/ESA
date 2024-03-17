using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using PROJETOESA.Data;
using PROJETOESA.Models;
using System.Diagnostics;
using System.Globalization;
using System.Text;

namespace PROJETOESA.Services
{
    public class FlightService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey = "ef9543daa9ed9c4a8cb31fca069c6107";
        private readonly string _googleApiKey = "AIzaSyD58yFxevJ8McI8Wc1WxUfx9EhVl-6D4gQ";
        private readonly AeroHelperContext _context;

        public FlightService(HttpClient httpClient, AeroHelperContext context)
        {
            _httpClient = httpClient;
            _context = context;
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

        public async Task<(string latitude, string longitude)> GetCoordinatesAsync(string locationName)
        {
            var city = _context.City.FirstOrDefault(c => c.Name == locationName);

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

        public async Task<string> GenerateMapUrl()
        {
            Debug.WriteLine("AQUI Serviçe!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
            string baseUrl = "https://maps.googleapis.com/maps/api/staticmap?";
            int mapWidth = 640;
            int mapHeight = 640;
            var mapUrl = new StringBuilder($"{baseUrl}size={mapWidth}x{mapHeight}&maptype=roadmap");

            List<FlightsItinerary> flights = await LoadFlightsAsync();

            foreach (var flight in flights)
            {
                var departureCoordinates = await GetCoordinatesAsync(flight.DepartureLocation.Name);
                var arrivalCoordinates = await GetCoordinatesAsync(flight.ArrivalLocation.Name);

                mapUrl.Append($"&markers=color:blue%7Clabel:S%7C{departureCoordinates.latitude},{departureCoordinates.longitude}");
                mapUrl.Append($"&markers=color:green%7Clabel:D%7C{arrivalCoordinates.latitude},{arrivalCoordinates.longitude}");

                mapUrl.Append($"&path=color:0xff0000ff|weight:5|{departureCoordinates.latitude},{departureCoordinates.longitude}|{arrivalCoordinates.latitude},{arrivalCoordinates.longitude}");
            }

            mapUrl.Append($"&key={_googleApiKey}");

            return mapUrl.ToString();
        }

        public async Task<string> GetAirlinesDataAsync()
        {
        
            var url = $"http://api.aviationstack.com/v1/airlines?access_key={_apiKey}";
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();

            return content;
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
