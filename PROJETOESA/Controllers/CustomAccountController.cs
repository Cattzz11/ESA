using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using PROJETOESA.Models;

namespace PROJETOESA.Controllers
{
    [ApiController]
    public class CustomAccountController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public CustomAccountController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        [HttpPost]
        [Route("api/register")]
        public async Task<IActionResult> Register([FromBody] CustomRegisterModel model)
        {
            var user = new ApplicationUser { UserName = model.Email, Email = model.Email, Name = model.Name };
            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {                
                return Ok();
            }

            return BadRequest(result.Errors);
        }

        [HttpPost("api/logout")]
        [Authorize] 
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);

            // Retorna uma resposta apropriada, como um status 200 OK
            return Ok(new { message = "Logout successful" });
        }

        [HttpPost]
        [Route("api/change-password")]
        public async Task<IActionResult> RecoverPassword([FromBody] CustomRecoverPasswordModel model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                return BadRequest("Utilizador não encontrado.");
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, model.Password);

            if (result.Succeeded)
            {
                return Ok();
            }

            return BadRequest(result.Errors);
        }
    }

    public class CustomRegisterModel
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        
    }

    public class CustomRecoverPasswordModel
    {
        public string Email { get; set; }
        public string Password { get; set; }

    }
}
