using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PROJETOESA.Models;
using Microsoft.AspNetCore.Identity.UI.Services;
using PROJETOESA.Services;
using Microsoft.EntityFrameworkCore;
using PROJETOESA.Data;
using System.Diagnostics;

namespace PROJETOESA.Controllers
{
    [ApiController]
    public class CustomAccountController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSender _emailSender;
        private readonly ILogger<CustomAccountController> _logger;
        private readonly CodeGeneratorService _codeGeneratorService;
        private readonly AeroHelperContext _context;


        public CustomAccountController(UserManager<ApplicationUser> userManager, IEmailSender emailSender, ILogger<CustomAccountController> logger, CodeGeneratorService codeGeneratorService, AeroHelperContext context)
        {
            _userManager = userManager;
            _emailSender = emailSender;
            _logger = logger;
            _codeGeneratorService = codeGeneratorService;
            _context = context;
        }

        [HttpPost]
        [Route("api/register")]
        public async Task<IActionResult> Register([FromBody] CustomRegisterModel model)
        {
            var user = new ApplicationUser { UserName = model.Email, Email = model.Email, Name = model.Name, Role = TipoConta.ClienteNormal };
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

        [HttpGet("api/userInfo")]
        public async Task<IActionResult> GetUserInfo()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            object userInfo;

            if (user.BirthDate.HasValue)
            {
                var today = DateTime.Today;
                var age = today.Year - user.BirthDate.Value.Year;

                if (user.BirthDate.Value > today.AddYears(-age)) age--;

                userInfo = new { UserName = user.UserName, Email = user.Email, Role = user.Role.ToString(), Name = user.Name, Age = age };
            }
            else
            {
                userInfo = new { UserName = user.UserName, Email = user.Email, Role = user.Role.ToString(), Name = user.Name };
            }

            return Ok(userInfo);
        }


        [HttpPost]
        [Route("api/send-recovery-code")]
        public async Task<IActionResult> SendRecoveryCode([FromBody] CustomRecoverModel model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user != null)
            {
                var confirmationCode = this._codeGeneratorService.GenerateCode();

                // Construção da mensagem para enviar
                var htmlContent = $"<p>Seu código de confirmação é: {confirmationCode}</p>";

                // Envio do e-mail
                await _emailSender.SendEmailAsync(model.Email, "Confirmação da Conta", htmlContent);

                // Armazenar o código de recuperação na base de dados
                await StoreRecoveryCodeAsync(model, confirmationCode);
            }

            return Ok(new { Message = "Se o registo for bem sucedido, o código de confirmação será enviado." });
        }

        [HttpPost]
        [Route("api/send-confirmation-code")]
        public async Task<IActionResult> SendConfirmationCode([FromBody] CustomRecoverModel model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user != null)
            {
                var recoveryCode = this._codeGeneratorService.GenerateCode();

                // Construção da mensagem para enviar
                var htmlContent = $"<p>Seu código de confirmação é: {recoveryCode}</p>";

                // Envio do e-mail
                await _emailSender.SendEmailAsync(model.Email, "Confirmação da Conta", htmlContent);

                // Armazenar o código de recuperação na base de dados
                await StoreConfirmationCodeAsync(model, recoveryCode);
            }

            return Ok(new { Message = "Se o e-mail existir, um e-mail de confirmação será enviado." });
        }

        private async Task StoreRecoveryCodeAsync(CustomRecoverModel user, string recoveryCode)
        {
            // Defenição do tempo de validade do código de recuperação, tempo = 5 Minutos.
            var expirationTime = DateTime.UtcNow.AddMinutes(5);

            var passwordRecoveryCode = new PasswordRecoveryCode
            {
                UserEmail = user.Email,
                Code = recoveryCode,
                ExpirationTime = expirationTime
            };

            _context.PasswordRecoveryCodes.Add(passwordRecoveryCode);
            await _context.SaveChangesAsync();
        }

        private async Task StoreConfirmationCodeAsync(CustomRecoverModel user, string code)
        {
            // Find the existing confirmation code for the user
            var existingCode = await _context.ConfirmationCodes
                .FirstOrDefaultAsync(c => c.UserEmail == user.Email);

            if (existingCode != null)
            {
                // Update the existing code with the new one
                existingCode.Code = code;
                existingCode.ExpirationTime = DateTime.UtcNow.AddMinutes(5);
            }
            else
            {
                // If no existing code, create a new one
                var expirationTime = DateTime.UtcNow.AddMinutes(5);

                var confirmationCode = new ConfirmationCode
                {
                    UserEmail = user.Email,
                    Code = code,
                    ExpirationTime = expirationTime
                };

                _context.ConfirmationCodes.Add(confirmationCode);
            }

            await _context.SaveChangesAsync();
        }

        [HttpPost]
        [Route ("api/validate-recovery-code")]
        public async Task<IActionResult> ValidateRecoveryCodeAsync(string userEmail, string code)
        {
            var recoveryCode = await _context.PasswordRecoveryCodes
                .FirstOrDefaultAsync(c => c.UserEmail == userEmail && c.Code == code);

            if (recoveryCode != null)
            {
                if (recoveryCode.ExpirationTime > DateTime.UtcNow)
                {
                    // O código é válido.
                    _context.PasswordRecoveryCodes.Remove(recoveryCode);
                    await _context.SaveChangesAsync();
                    return Ok(new { Message = "Código válido." });
                }
                else
                {
                    // O código expirou.
                    _context.PasswordRecoveryCodes.Remove(recoveryCode);
                    await _context.SaveChangesAsync();
                    return BadRequest(new { Message = "Código expirado." });
                }
            }

            // O código é inválido.
            return BadRequest(new { Message = "Código inválido." });
        }

        [HttpPost]
        [Route("api/validate-confirmation-code")]
        public async Task<IActionResult> ValidateConfirmationCodeAsync(string userEmail, string code)
        {
            var confirmationCode = await _context.ConfirmationCodes
                .FirstOrDefaultAsync(c => c.UserEmail == userEmail && c.Code == code);

            if (confirmationCode != null)
            {
                if (confirmationCode.ExpirationTime > DateTime.UtcNow)
                {
                    // O código é válido.
                    _context.ConfirmationCodes.Remove(confirmationCode);
                    await _context.SaveChangesAsync();
                    return Ok(new { Message = "Código válido." });
                }
                else
                {
                    // O código expirou.
                    _context.ConfirmationCodes.Remove(confirmationCode);
                    await _context.SaveChangesAsync();
                    return BadRequest(new { Message = "Código expirado." });
                }
            }

            // O código é inválido.
            return BadRequest(new { Message = "Código inválido." });
        }

        [HttpGet]
        [Route("api/check-email-confirmation-status")]
        [AllowAnonymous]
        public async Task<IActionResult> CheckEmailConfirmationStatus([FromQuery] string email)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user != null)
            {
                var emailConfirmed = await _userManager.IsEmailConfirmedAsync(user);
                return Ok(emailConfirmed);
            }

            return BadRequest(new { Message = "User not found." });
        }

        [HttpGet]
        [Route("api/check-email-exists")]
        [AllowAnonymous]
        public async Task<IActionResult> CheckEmailExistsInDB([FromQuery] string email)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user != null)
            {
                return Ok();
            }

            return BadRequest(new { Message = "User not found." });
        }

        [HttpPost]
        [Route("api/update-confirmed-email")]
        public async Task<IActionResult> UpdateConfirmedEmail([FromBody] UpdateConfirmedEmailModel model)
        {
            // Find the user in the database based on the email
            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user != null)
            {
                // Update the confirmedEmail status
                user.EmailConfirmed = true;
                var result = await _userManager.UpdateAsync(user);

                if (result.Succeeded)
                {
                    return Ok(new { Message = "Email confirmed successfully." });
                }
            }

            return BadRequest(new { Message = "Failed to confirm email." });
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

    public class CustomRecoverPasswordModel
    {
        public string Email { get; set; }
        public string Password { get; set; }

    }
    public class UpdateConfirmedEmailModel
    {
        public string Email { get; set; }
    }
}
