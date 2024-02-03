using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using PROJETOESA.Models;
using System.Text.Encodings.Web;
using System.Text;
using Microsoft.AspNetCore.Identity.UI.Services;

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity.UI.V4.Pages.Account.Internal;
using System.Net;

namespace PROJETOESA.Controllers
{
    [ApiController]
    public class CustomAccountController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSender _emailSender;
        private readonly ILogger<CustomAccountController> _logger;


        public CustomAccountController(UserManager<ApplicationUser> userManager, IEmailSender emailSender, ILogger<CustomAccountController> logger)
        {
            _userManager = userManager;
            _emailSender = emailSender;
            _logger = logger;

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
        [Route("api/forgotPassword")]
        public async Task<IActionResult> RecoverPassword([FromBody] CustomRecoverModel model)
        {
            // Validate model, find user by email, generate token, etc.
            try
            {

                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null)
                {
                    // User not found
                    return NotFound();
                }

                // Generate password reset token
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);

                // Send recovery email
                var emailSubject = "Password Recovery";
                var emailBody = $"Click the following link to reset your password: {GenerateResetLink(user.Id, token)}";

                await _emailSender.SendEmailAsync(user.Email, emailSubject, emailBody);

                _logger.LogInformation($"Password recovery email sent to {user.Email}.");

                return Ok();
            }

            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while recovering password.");
                return StatusCode(500, "Internal Server Error");
            }
        }

        private string GenerateResetLink(string userId, string token)
        {
            // Replace the following URL with the actual URL of your password reset page
            string resetPasswordUrl = "https://localhost:4200/resetPassword";

            // Construct the reset link with placeholders for userId and token
            string resetLink = $"{resetPasswordUrl}?userId={userId}&token={WebUtility.UrlEncode(token)}";

            return resetLink;
        }


    }

    public class CustomRegisterModel
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        
    }


    public class CustomRecoverModel
    { 
    public string Email { get; set; }

    }
}
