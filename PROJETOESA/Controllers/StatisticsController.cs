using Microsoft.AspNetCore.Mvc;
using PROJETOESA.Services;

namespace PROJETOESA.Controllers
{
    [ApiController]

    public class StatisticsController : ControllerBase
    {
        private readonly StatisticsService _statisticsService;

        public StatisticsController(StatisticsService statisticsService)
        {
            _statisticsService = statisticsService;
        }

        [HttpGet]
        [Route("api/statistics")]
        public async Task<IActionResult> GetStatistics()
        {
            var statistics = await _statisticsService.GetStatisticsAsync();
            return Ok(statistics);
        }

        [HttpGet]
        [Route("api/logins")]
        public async Task<IActionResult> GetLoginsByDate([FromQuery] DateTime date)
        {
            var loginCount = await _statisticsService.GetLoginsByDateAsync(date);
            return Ok(loginCount);
        }

        [HttpGet]
        [Route("api/maxLogins")]
        public async Task<IActionResult> GetMaxLogins()
        {
            var maxLogins = await _statisticsService.GetMaxDailyLoginsAsync();
            return Ok(maxLogins);
        }

        [HttpGet]
        [Route("api/maxRegistrations")]
        public async Task<IActionResult> GetMaxRegistrations()
        {
            var maxRegistrations = await _statisticsService.GetMaxDailyRegistrationsAsync();
            return Ok(maxRegistrations);
        }
    }
}
