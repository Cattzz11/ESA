using Microsoft.AspNetCore.Mvc;
using PROJETOESA.Models;
using PROJETOESA.Services;
using System.Diagnostics;

namespace PROJETOESA.Controllers
{
    [ApiController]
    public class DataController : Controller
    {
        private readonly DataService _dataService;

        public DataController(DataService dataService)
        {
            _dataService = dataService;
        }

        [HttpGet]
        [Route("api/data/flight-by-user")]
        public async Task<IActionResult> getFlightsByUser([FromQuery] string userId)
        {
            Debug.WriteLine("AQUI");
            List<TripDto> result = await _dataService.GetFlightsByUserAsync(userId);

            return Ok(result);
        }

        [HttpGet]
        [Route("api/data/GetAllCountries")]
        public async Task<IActionResult> GetCountries()
        {
            List<Country> result = await _dataService.GetAllCountriesAsync();

            return Ok(result);
        }

        [HttpGet]
        [Route("api/data/GetAllCities")]
        public async Task<IActionResult> GetCities()
        {
            List<City> result = await _dataService.GetAllCitiesAsync();

            return Ok(result);
        }
    }
}