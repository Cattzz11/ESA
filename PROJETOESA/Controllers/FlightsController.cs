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

        /// <summary>
        /// Método que dá todas as opções de preço que existem para um determinado voo e faz o min, max e média
        /// </summary>
        /// <param name="data">Dados necessários para a observação destes preços (obrigatórios: departureAPIKey, destinationAPIKey, departureDate, arrivalDate)</param>
        /// <returns>Opções de preços num array - min, max, média</returns>
        [HttpGet]
        [Route("api/flight/price-options")]
        public async Task<IActionResult> PriceOptions([FromQuery] FlightData data)
        {
            var prices = await _skyscannerService.GetRoundtripPricesAsync(data);
            Console.WriteLine("array preços", prices);

            if (prices.Length == 0)
            {
                return NotFound("Nenhum preço encontrado.");
            }

            double minPrice = prices.Min();
            double maxPrice = prices.Max();
            double averagePrice = Math.Round(prices.Average(),2);

            var result = new
            {
                MinPrice = minPrice,
                MaxPrice = maxPrice,
                AveragePrice = averagePrice
            };

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
            //List<Trip> result = await _skyscannerService.GetSugestionsCompanyAsyncTest();

            //Debug.WriteLine(result.ToString());
            List<Trip> result = await _skyscannerService.GetSugestionsCompanyAsync(carrierId);

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
