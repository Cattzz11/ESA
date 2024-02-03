using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PROJETOESA.Models;
using Microsoft.AspNetCore.Identity.UI.Services;
using PROJETOESA.Services;

namespace PROJETOESA.Controllers
{
    [ApiController]
    public class CustomAccountController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSender _emailSender;
        private readonly ILogger<CustomAccountController> _logger;
        private readonly CodeGeneratorService _codeGeneratorService;


        public CustomAccountController(UserManager<ApplicationUser> userManager, IEmailSender emailSender, ILogger<CustomAccountController> logger, CodeGeneratorService codeGeneratorService)
        {
            _userManager = userManager;
            _emailSender = emailSender;
            _logger = logger;
            _codeGeneratorService = codeGeneratorService;
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
                //var token = await _userManager.GeneratePasswordResetTokenAsync(user);

                // Send recovery email
                //var emailSubject = "Password Recovery";
                //var emailBody = $"Click the following link to reset your password: {GenerateResetLink(user.Id, token)}";

                //await _emailSender.SendEmailAsync(user.Email, emailSubject, emailBody);

                _logger.LogInformation($"Password recovery email sent to {user.Email}.");

                return Ok();
            }

            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while recovering password.");
                return StatusCode(500, "Internal Server Error");
            }
        }

        [HttpPost]
        [Route("api/send-recovery-code")]
        public async Task<IActionResult> SendRecoveryCode([FromBody] CustomRecoverModel model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user != null)
            {
                var recoveryCode = this._codeGeneratorService.GenerateCode();

                // Construa a mensagem de e-mail
                var htmlContent = $"<p>Seu código de recuperação é: {recoveryCode}</p>";

                // Envie o e-mail
                await _emailSender.SendEmailAsync(model.Email, "Recuperação de Senha", htmlContent);

            }

            return Ok(new { Message = "Se o e-mail estiver registrado, um e-mail de recuperação será enviado." });
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
