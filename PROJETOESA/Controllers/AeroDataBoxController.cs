using Microsoft.AspNetCore.Mvc;
using PROJETOESA.Services.AeroDataBoxService;
using PROJETOESA.Services.EmailService;
using System.Diagnostics;

namespace PROJETOESA.Controllers
{
    [ApiController]
    public class AeroDataBoxController : Controller
    {

        private readonly IAeroDataBoxService _aeroData;
        private readonly IEmailService _email;

        public AeroDataBoxController(IAeroDataBoxService aeroData, IEmailService email)
        {
            _aeroData = aeroData;
            _email = email;
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
