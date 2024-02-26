﻿using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PROJETOESA.Models;
using Microsoft.AspNetCore.Identity.UI.Services;
using PROJETOESA.Services;
using Microsoft.EntityFrameworkCore;
using PROJETOESA.Data;
using System.Diagnostics;
using Microsoft.IdentityModel.Tokens;

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

            return Ok(user);
        }


        [HttpPost]
        [Route("api/send-recovery-code")]
        public async Task<IActionResult> SendRecoveryCode([FromBody] CustomRecoverModel model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user != null)
            {
                var recoveryCode = this._codeGeneratorService.GenerateCode();

                // Construção da mensagem para enviar
                var htmlContent = $"<p>Seu código de recuperação é: {recoveryCode}</p>";

                // Envio do e-mail
                await _emailSender.SendEmailAsync(model.Email, "Recuperação de Senha", htmlContent);

                // Armazenar o código de recuperação na base de dados
                await StoreRecoveryCodeAsync(model, recoveryCode);
            }

            return Ok(new { Message = "Se o e-mail estiver registrado, um e-mail de recuperação será enviado." });
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
}
