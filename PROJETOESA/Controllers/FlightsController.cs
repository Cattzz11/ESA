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
    }
}
