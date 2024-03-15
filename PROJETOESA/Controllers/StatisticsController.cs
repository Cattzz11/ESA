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
        [Route("/statistics")]
        public async Task<IActionResult> GetStatistics()
        {
            var statistics = await _statisticsService.GetStatisticsAsync();
            return Ok(statistics);
        }
    }
}
