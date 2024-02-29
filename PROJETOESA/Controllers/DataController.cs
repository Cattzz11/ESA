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
            List<TripDto> result = await _dataService.GetFlightsByUserAsync(userId);

            return Ok(result);
        }

        [HttpGet]
        [Route("api/data/all-cities-and-countries")]
        public async Task<IActionResult> getAllCities()
        {
            List<CityDto> result = await _dataService.GetAllCitiesAsync();

            return Ok(result);
        }

        [HttpGet]
        [Route("api/data/PopulateBDCountries")]
        public async Task<IActionResult> PopulateBDCountries()
        {
            List<Country> result = await _dataService.PopulateBDCountries();

            return Ok(result);
        }

        [HttpGet]
        [Route("api/data/PopulateBDCity")]
        public async Task<IActionResult> PopulateBDCity()
        {
            List<City> result = await _dataService.PopulateBDCity();

            return Ok(result);
        }
    }
}