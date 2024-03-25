using Newtonsoft.Json.Linq;
using PROJETOESA.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace PROJETOESA.Services.AeroDataBoxService
{
    public class AeroDataBoxService : IAeroDataBoxService
    {
        private readonly HttpClient _httpClient;
        private readonly HttpClient _httpClient2;

        public AeroDataBoxService(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("AeroDataBoxClient");
            _httpClient2 = httpClientFactory.CreateClient("FlighteraFlight");
        }

        public async Task<AircraftData> GetFlightStatusAsync(string flightIATA)
        {
            var response = await _httpClient2.GetAsync($"flight/info?flnr={flightIATA}");
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();

            var flightsArray = JArray.Parse(content);

            if (flightsArray.Count > 0)
            {
                var flight = flightsArray[0];
                if (flight != null)
                {
                    var aircraftData = new AircraftData
                    {
                        Registration = flight["reg"]?.ToString()
                    };

                    return await GetAircraftDataAsync(aircraftData);
                }
            }

            return null;
        }

        public async Task<AircraftData> GetFlightStatusTestAsync(string flightIATA)
        {
            var aircraftData = new AircraftData
            {
                Model = "Boeing 787",
                Registration = "TC-LLZ",
                IsActive = true,
                Airline = "Turkish Airlines",
                ICAO = "B789",
                ModelCode = "787-9",
                SeatsNumber = 300,
                RolloutDate = null,
                FirstFlightDate = new DateTime(2023, 12, 12),
                RegistrationDate = new DateTime(2023,12,12),
                EnginesNumber = 2,
                EnginesType = "Jet",
                IsFreighter = false,
                ProductionLine = "Boeing 787",
                Age = 0,
                NumRegistrations = 1,
                Photo = "https://farm66.staticflickr.com/65535/53473858589_d0dc21a0c8_z.jpg"
            };

            return aircraftData;
        }

        private async Task<AircraftData> GetAircraftDataAsync(AircraftData data)
        {
            var response = await _httpClient.GetAsync($"aircrafts/reg/{data.Registration}?withRegistrations=true&withImage=true");

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();

            var aircraftJson = JObject.Parse(content);

            data.Model = aircraftJson["productionLine"]?.ToString();
            data.IsActive = (bool)aircraftJson["active"];
            data.Airline = aircraftJson["airlineName"]?.ToString();
            data.ICAO = aircraftJson["icaoCode"]?.ToString();
            data.ModelCode = aircraftJson["modelCode"]?.ToString();
            data.SeatsNumber = (int)aircraftJson["numSeats"];

            DateTime tempDate;
            data.RolloutDate = DateTime.TryParse(aircraftJson["rolloutDate"]?.ToString(), out tempDate) ? tempDate : (DateTime?)null;
            data.FirstFlightDate = DateTime.TryParse(aircraftJson["firstFlightDate"]?.ToString(), out tempDate) ? tempDate : DateTime.TryParse(aircraftJson["deliveryDate"]?.ToString(), out tempDate) ? tempDate : (DateTime?)null;
            data.RegistrationDate = DateTime.TryParse(aircraftJson["registrationDate"]?.ToString(), out tempDate) ? tempDate : (DateTime?)null;

            data.EnginesNumber = (int)aircraftJson["numEngines"];
            data.EnginesType = aircraftJson["engineType"]?.ToString();
            data.IsFreighter = (bool)aircraftJson["isFreighter"];
            data.ProductionLine = aircraftJson["productionLine"]?.ToString();
            data.Age = (int)Math.Round((decimal)aircraftJson["ageYears"]);
            data.NumRegistrations = (int)aircraftJson["numRegistrations"];
            data.Photo = aircraftJson["image"]["url"]?.ToString();

            return data;
        }
    }
}
