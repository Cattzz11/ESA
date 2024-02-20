using Microsoft.AspNetCore.Mvc;
using PROJETOESA.Models;
using PROJETOESA.Services;
using System.Diagnostics;
using System.Threading.Tasks;

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
        public async Task<IActionResult> searchRoundtrip([FromQuery] FlightData data)
        {
            var result = await _skyscannerService.GetRoundtripAsync(data);

            return Ok(result);
        }

        // Pesquisa todas as viagens
        [HttpGet]
        [Route("api/search-everywhere")]
        public async Task<IActionResult> searchEverywhere([FromQuery] FlightData data)
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
    }


}
