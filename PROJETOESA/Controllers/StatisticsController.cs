using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PROJETOESA.Models;
using PROJETOESA.Services;

namespace PROJETOESA.Controllers
{
    [ApiController]

    public class StatisticsController : ControllerBase
    {
        private readonly StatisticsService _statisticsService;
        private readonly UserManager<ApplicationUser> _userManager;

        public StatisticsController(StatisticsService statisticsService, UserManager<ApplicationUser> userManager)
        {
            _statisticsService = statisticsService;
            _userManager = userManager;
        }

        [HttpGet]
        [Route("api/statistics")]
        [Authorize]
        public async Task<IActionResult> GetStatistics()
        {
            var user = await _userManager.FindByEmailAsync(User?.Identity?.Name ?? string.Empty);

            if (user == null)
            {
                return NotFound();
            }

            if (user.Role == TipoConta.Administrador)
            {
                var statistics = await _statisticsService.GetStatisticsAsync();
                return Ok(statistics);
            }

            return NotFound();
        }

        [HttpGet]
        [Route("api/logins")]
        [Authorize]
        public async Task<IActionResult> GetLoginsByDate([FromQuery] DateTime date)
        {
            var user = await _userManager.FindByEmailAsync(User?.Identity?.Name ?? string.Empty);

            if (user == null)
            {
                return NotFound();
            }

            if (user.Role == TipoConta.Administrador)
            {
                var loginCount = await _statisticsService.GetLoginsByDateAsync(date);
                return Ok(loginCount);
            }
            
            return NotFound();
        }

        [HttpGet]
        [Route("api/maxLogins")]
        [Authorize]
        public async Task<IActionResult> GetMaxLogins()
        {
            var user = await _userManager.FindByEmailAsync(User?.Identity?.Name ?? string.Empty);

            if (user == null)
            {
                return NotFound();
            }

            if (user.Role == TipoConta.Administrador)
            {
                var maxLogins = await _statisticsService.GetMaxDailyLoginsAsync();
                return Ok(maxLogins);
            }

            return NotFound();
        }

        [HttpGet]
        [Route("api/maxRegistrations")]
        [Authorize]
        public async Task<IActionResult> GetMaxRegistrations()
        {
            var user = await _userManager.FindByEmailAsync(User?.Identity?.Name ?? string.Empty);

            if (user == null)
            {
                return NotFound();
            }

            if (user.Role == TipoConta.Administrador)
            {
                var maxRegistrations = await _statisticsService.GetMaxDailyRegistrationsAsync();
                return Ok(maxRegistrations);
            }

            return NotFound();
        }
    }
}
