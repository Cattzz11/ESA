using Microsoft.AspNetCore.Mvc;
using PROJETOESA.Models.ViewModels;
using PROJETOESA.Services.AeroDataBoxService;
using System.Diagnostics;

namespace PROJETOESA.Controllers
{
    [ApiController]
    public class AeroDataBoxController : Controller
    {

        private readonly IAeroDataBoxService _aeroData;

        public AeroDataBoxController(IAeroDataBoxService aeroData)
        {
            _aeroData = aeroData;
        }

        [HttpGet]
        [Route("api/aero-dataBox/flight-status")]
        public async Task<IActionResult> GetFlightStatus(string flightIATA)
        {
            //var result = await _aeroData.GetFlightStatusAsync(flightIATA);
            var result = await _aeroData.GetFlightStatusTestAsync(flightIATA);

            Debug.WriteLine(result.Photo);
            return Ok(result);
        }
    }
}
