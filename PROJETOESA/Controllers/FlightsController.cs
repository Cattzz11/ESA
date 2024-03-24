using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PROJETOESA.Data;
using PROJETOESA.Models;
using PROJETOESA.Models.ViewModels;
using PROJETOESA.Services.SkyscannerService;


namespace PROJETOESA.Controllers
{
    [ApiController]
    public class FlightsController : Controller
    {
        private readonly ISkyscannerService _skyscannerService;
        private readonly AeroHelperContext _context;

        public FlightsController(ISkyscannerService skyscannerService, AeroHelperContext context)
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

        [HttpGet]
        [Route("api/flight/search-roundtrip-premium")]
        public async Task<IActionResult> SearchRoundtripPremium([FromQuery] FlightData data)
        {
            var result = await _skyscannerService.GetRoundtripPremiumAsync(data);

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
            var result = await _skyscannerService.GetDataAsync(data.Name);

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
        [Route("api/data/favourite-destinations")]
        public async Task<IActionResult> getFavouriteDestinations()
        {
            List<City> result = await _skyscannerService.GetFavouriteDestinationsAsync();

            return Ok(result);
        }

        [HttpGet]
        [Route("api/flight/sugestions-destinations")]
        public async Task<IActionResult> getSugestionsDestinations()
        {
            List<Trip> result = await _skyscannerService.GetSugestionsDestinationsAsync();

            return Ok(result);
        }

        [HttpGet]
        [Route("api/flight/trip-details")]
        public async Task<IActionResult> getTripDetails(string token, string itineraryId)
        {
            TripDetailsViewModel result = await _skyscannerService.GetTripDetailsAsync(token, itineraryId);

            return Ok(result);
        }
    }
}
