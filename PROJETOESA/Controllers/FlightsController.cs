using Microsoft.AspNetCore.Mvc;
using PROJETOESA.Models;
using PROJETOESA.Services;
using System.Diagnostics;


namespace PROJETOESA.Controllers
{
    [ApiController]
    public class FlightsController : Controller
    {
        private readonly SkyscannerService _skyscannerService;

        public FlightsController(SkyscannerService skyscannerService)
        {
            _skyscannerService = skyscannerService;
        }

        // Viagens de ida e volta
        [HttpGet]
        [Route("api/search-roundtrip")]
        public async Task<IActionResult> SearchRoundtrip([FromQuery] FlightData data)
        {
            var result = await _skyscannerService.GetRoundtripAsync(data);
            
            return Ok(result);
        }

        // Pesquisa todas as viagens
        [HttpGet]
        [Route("api/search-everywhere")]
        public async Task<IActionResult> SearchEverywhere([FromQuery] FlightData data)
        {
            var result = await _skyscannerService.GetEverywhereAsync(data);

            return Ok(result);
        }


        // Viagem apenas de ida
        [HttpGet]
        [Route("api/search-one-way")]
        public async Task<IActionResult> searchOneWayTrip([FromQuery] FlightData data)
        {
            var result = await _skyscannerService.GetOneWayAsync(data);

            return Ok(result);
        }

        // Calendário com os preços
        [HttpGet]
        [Route("api/price-calendar")]
        public async Task<IActionResult> searchPriceCalendar([FromQuery] FlightData data)
        {
            var result = await _skyscannerService.GetCalendarAsync(data);

            return Ok(result);
        }

        [HttpGet]
        [Route("api/auto-complete")]
        public async Task<IActionResult> searchData([FromQuery] Country data)
        {
            var result = await _skyscannerService.GetDataAsync(data);

            return Ok(result);
        }

        [HttpGet]
        [Route("api/airport-list")]
        public async Task<IActionResult> getAirportList([FromQuery] Country data)
        {
            var result = await _skyscannerService.GetAirportListAsync(data);

            return Ok(result);
        }

        [HttpGet]
        [Route("api/sugestions-company")]
        public async Task<IActionResult> getSugestionsCompany([FromQuery] string carrierName)
        {
            List<Itinerary> result = await _skyscannerService.GetSugestionsCompanyAsync(carrierName);

            return Ok(result);
        }

        [HttpGet]
        [Route("api/favorite-airline")]
        public async Task<IActionResult> getFavoriteAirline()
        {
            List<Carrier> result = await _skyscannerService.GetFavoriteAirlineAsync();

            return Ok(result);
        }
    }
}
