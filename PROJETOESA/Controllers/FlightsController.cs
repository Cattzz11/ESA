using Microsoft.AspNetCore.Mvc;
using PROJETOESA.Models;
using PROJETOESA.Services;
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

        [HttpGet]
        [Route("api/getFlights")]
        public async Task<IActionResult> Get([FromQuery] FlightData data)
        {
            var result = await _skyscannerService.GetFlightsAsync(data);
            return Ok(result);
        }
    }


}
