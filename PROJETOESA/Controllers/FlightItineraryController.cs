using Microsoft.AspNetCore.Mvc;
using PROJETOESA.Models;
using PROJETOESA.Services.FlightService;
using System.Diagnostics;

namespace PROJETOESA.Controllers
{
    [ApiController]
    public class FlightItineraryController : Controller
    {
        private readonly IFlightService _flightService;

        public FlightItineraryController(IFlightService flightService)
        {
            _flightService = flightService;
        }

        [HttpGet]
        [Route("api/flight-itinerary/live-flights")]
        public async Task<IActionResult> GetLiveFlights()
        {
            //var data = await _flightService.GetLiveFlightData();
            var data = await _flightService.LoadFlightsAsync();
            return Ok(data);
        }

        [HttpGet]
        [Route("api/flight-itinerary/airlines")]
        public async Task<IActionResult> GetAirLineData()
        {
            var data = await _flightService.GetAirlinesDataAsync();
            return Ok(data);
        }

        [HttpGet]
        [Route("api/flight-itinerary/generate-map")]
        public async Task<IActionResult> CreateMap()
        {
            string data = await _flightService.GenerateMapUrl();

            return Ok(data);
        }

        [HttpGet]
        [Route("api/flight-itinerary/search-flights")]
        public async Task<IActionResult> SearchFlights([FromQuery] AddressComponents origin, [FromQuery] AddressComponents destination)
        {
            var data = await _flightService.GetFlightsAsync(origin, destination);
            return Ok(data);
        }

        [HttpGet]
        [Route("api/flight-itinerary/search-flights-premium")]
        public async Task<IActionResult> SearchFlightsPremium([FromQuery] AddressComponents origin, [FromQuery] AddressComponents destination)
        {
            Debug.WriteLine("Controller");
            var data = await _flightService.GetFlightsPremiumAsync(origin, destination);
            return Ok(data);
        }

        [HttpGet]
        [Route("api/flight-itinerary/populate-coordinates")]
        public async Task<IActionResult> PopulateCoordinates()
        {
            var data = await _flightService.PopulateCoordinatesAsync();
            return Ok(data);
        }

    }

    public class AddressComponents
    {
        public string city { get; set; }
        public string country { get; set; }
        public string latitude { get; set; }
        public string longitude { get; set; }
    }
}


