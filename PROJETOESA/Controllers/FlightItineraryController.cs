using Microsoft.AspNetCore.Mvc;
using PROJETOESA.Services;
using System.Diagnostics;

namespace PROJETOESA.Controllers
{
    [ApiController]
    public class FlightItineraryController : Controller
    {
        private readonly FlightService _flightService;

        public FlightItineraryController(FlightService flightService)
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
    }
}
