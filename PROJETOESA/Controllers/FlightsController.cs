using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PROJETOESA.Data;
using PROJETOESA.Models;
using PROJETOESA.Services;
using System.Diagnostics;


namespace PROJETOESA.Controllers
{
    [ApiController]
    public class FlightsController : Controller
    {
        private readonly SkyscannerService _skyscannerService;
        private readonly AeroHelperContext _context;

        public FlightsController(SkyscannerService skyscannerService, AeroHelperContext context)
        {
            _skyscannerService = skyscannerService;
            _context = context;
        }

        // Viagens de ida e volta
        [HttpGet]
        [Route("api/flight/search-roundtrip")]
        public async Task<IActionResult> SearchRoundtrip([FromQuery] FlightData data)
        {
            var result = await _skyscannerService.GetRoundtripAsync(data);

            return Ok(result);
        }

        // Pesquisa todas as viagens
        [HttpGet]
        [Route("api/flight/search-everywhere")]
        public async Task<IActionResult> SearchEverywhere([FromQuery] FlightData data)
        {
            var result = await _skyscannerService.GetEverywhereAsync(data);

            return Ok(result);
        }


        // Viagem apenas de ida
        [HttpGet]
        [Route("api/flight/search-one-way")]
        public async Task<IActionResult> searchOneWayTrip([FromQuery] FlightData data)
        {
            var result = await _skyscannerService.GetOneWayAsync(data);

            return Ok(result);
        }

        // Calendário com os preços
        [HttpGet]
        [Route("api/flight/price-calendar")]
        public async Task<IActionResult> searchPriceCalendar([FromQuery] FlightData data)
        {
            List<Calendar> result = await _skyscannerService.GetCalendarAsync(data);

            return Ok(result);
        }

        [HttpGet]
        [Route("api/flight/auto-complete")]
        public async Task<IActionResult> searchData([FromQuery] Country data)
        {
            var result = await _skyscannerService.GetDataAsync(data);

            return Ok(result);
        }

        [HttpGet]
        [Route("api/flight/airport-list")]
        public async Task<IActionResult> getAirportList([FromQuery] Country data)
        {
            var result = await _skyscannerService.GetAirportListAsync(data);

            return Ok(result);
        }

        [HttpGet]
        [Route("api/flight/sugestions-company")]
        public async Task<IActionResult> getSugestionsCompany([FromQuery] string carrierId)
        {
            List<Trip> result = await _skyscannerService.GetSugestionsCompanyAsyncTest();

            Debug.WriteLine(result.ToString());
            //List<Trip> result = await _skyscannerService.GetSugestionsCompanyAsync(carrierId);

            return Ok(result);
        }

        [HttpGet]
        [Route("api/flight/favorite-airline")]
        public async Task<IActionResult> getFavoriteAirline()
        {
            List<Carrier> result = await _skyscannerService.GetFavoriteAirlineAsync();

            return Ok(result);
        }


        [HttpGet]
        [Route("api/trip-details/{tripId}")]
        public async Task<IActionResult> GetTripDetails(string flightId)
        {
            Console.WriteLine($"Mimimimi");
            var flightDetail = await _context.Flights.FirstOrDefaultAsync(c => c.Id == flightId);
            Console.WriteLine($"Detail: {flightDetail}");
            var tripId = flightDetail;
            /*var tripDetail = await _context.Trip.
                FirstOrDefaultAsync(c => c.Id == tripId);*/

            

            var price = 0 ;

            Console.WriteLine($"Preço: {price}");

            try
            {
                // Retrieve trip details from the database based on tripId
                // Example: var tripDetails = await _context.TripDetails.FindAsync(tripId);

                // If tripDetails is null, return NotFound
                if (tripId == null)
                {
                    return NotFound(new { Message = "Trip details not found" });
                }

                // Optionally, you can perform additional processing or validation here

                return Ok(new { Message = $"Preço: {price}" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = "Failed to retrieve trip details" });
            }
        }
    }
}
