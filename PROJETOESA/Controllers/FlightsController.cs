using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PROJETOESA.Data;
using PROJETOESA.Models;
using PROJETOESA.Services;
using Square.Models;
using System.Diagnostics;


namespace PROJETOESA.Controllers
{
    [ApiController]
    public class FlightsController : Controller
    {
        private readonly SkyscannerService _skyscannerService;
        private readonly AeroHelperContext _context;
        //private readonly SquareService _squareService;

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
        public async Task<IActionResult> GetTripDetails(string tripId)
        {
            try
            {
                var flightDetail = await _context.Flights.FirstOrDefaultAsync(c => c.Id == tripId);
                var dbTripId = flightDetail?.TripId;
                var originCityDetail = await _context.City.FirstOrDefaultAsync(city => city.Id == flightDetail.OriginCityId);
                var destinationCityDetail = await _context.City.FirstOrDefaultAsync(city => city.Id == flightDetail.DestinationCityId);
                var tripDetail = await _context.Trip.FirstOrDefaultAsync(c => c.Id == dbTripId);
                var price = tripDetail.Price;


                if (flightDetail == null)
                {
                    // Return a NotFound response with details
                    return NotFound(new { Message = "Flight details not found", TripId = tripId });
                }

                // Create an instance of TripDetailsModel with the desired properties
                var tripDetailsModel = new TripDetailsModel
                {
                    Id = dbTripId,
                    DepartureDate = flightDetail.Departure.Date,
                    DepartureTime = flightDetail.Departure.Hour + ":" + flightDetail.Departure.Minute,
                    OriginCity = originCityDetail.Name,
                    Duration = flightDetail.Duration,
                    ArrivalDate = flightDetail.Arrival.Date,
                    ArrivalTime = flightDetail.Arrival.Hour + ":" + flightDetail.Arrival.Minute,
                    DestinationCity = destinationCityDetail.Name,
                    Price = price
                };

                // Optionally, you can perform additional processing or validation here

                // Return an Ok response with the TripDetailsModel
                return Ok(tripDetailsModel);
            }
            catch (Exception ex)
            {

                // Return a BadRequest response with details
                return BadRequest(new { Message = "Failed to retrieve trip details", Error = ex.Message });
            }
        }

        [HttpPost]
        [Route("api/purchase-ticket/{tripId}")]
        //[Authorize]
        public async Task<IActionResult> PurchaseTicket([FromBody] PaymentModel request, string tripId)
        {
            try
            {
                // Validate the request
                if (request == null)
                {
                    return BadRequest(new { Message = "Invalid request" });
                }

               // _squareService.PayAsync(request);

                await _context.SaveChangesAsync();

                // Return a success response
                return Ok(new { Message = "Ticket purchased successfully" });

                
            }
            catch (Exception ex)
            {
                // Return an error response with details
                return BadRequest(new { Message = "Failed to purchase ticket", Error = ex.Message });
            }
        }


    }

    public class TripDetailsModel
    {
        public string Id { get; set; }
        public DateTime DepartureDate { get; set; }
        public string DepartureTime { get; set; }
        public string OriginCity { get; set; }
        public string Duration { get; set; }
        public DateTime ArrivalDate { get; set; }
        public string ArrivalTime { get; set; }
        public string DestinationCity { get; set; }
        public double Price { get; set; }
        // Add other properties as needed
    }

}
