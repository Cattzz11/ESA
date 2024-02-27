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
using Microsoft.DotNet.Scaffolding.Shared.Messaging;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace PROJETOESA.Controllers
{
    [ApiController]
    [Produces("application/json")]
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

                userInfo = new { UserName = user.UserName, Email = user.Email, Role = user.Role.ToString(), Name = user.Name, Age = age, Nationality = user.Nationality, Occupation = user.Occupation, Gender = user.Gender };
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

        //GET:
        [HttpPut("api/edit-profile")]
        public async Task<IActionResult> UpdateUserInfo([FromBody] EditUserModel model)
        {
            Debug.WriteLine("SERVIDOR");
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound("User not found");
            }

            //atualizar os dados do user
            user.Name = model.Name;
            user.Email = model.Email;
            user.BirthDate = model.BirthDate;
            user.Occupation = model.Occupation;
            user.Nationality = model.Nationality;
            user.ProfilePicture = model.ProfilePicture;
            user.Gender = model.Gender;
            
            Debug.WriteLine("DADOS ATUALIZADOS ZÉ");

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                await _context.SaveChangesAsync();
                Debug.WriteLine("SALVAR ALTERAÇÕES");
                return Ok(new { Message = "Perfil atualizado com sucesso." });
            }
            return BadRequest(result.Errors);
        }

        [HttpGet("api/users")]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _context.Users.ToListAsync();
            return Ok(users);
        }

        [HttpDelete("api/users/{email}")]
        public async Task<IActionResult> DeleteUser(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
                return NotFound(); // User not found
            }

            var result = await _userManager.DeleteAsync(user);

            if (result.Succeeded)
            {
                return NoContent(); // User deleted successfully
            }
            else
            {
                return BadRequest(result.Errors); // Error deleting user
            }
        }

        //[HttpPost("api/upload-photo")]
        //public async Task<IActionResult> UploadPhoto(IFormFile file)
        //{
        //    if (file == null || file.Length == 0)
        //    {
        //        return BadRequest("Nenhum arquivo enviado.");
        //    }

        //    using (var memoryStream = new MemoryStream())
        //    {
        //        await file.CopyToAsync(memoryStream);

        //        // Obtenha o usuário atual (você precisará do UserManager configurado em seu controleador)
        //        var user = await _userManager.GetUserAsync(User);

        //        // Atualize o campo ProfilePictureBinary do usuário com os dados da imagem
        //        user.ProfilePictureBinary = memoryStream.ToArray();

        //        // Atualize o usuário no banco de dados
        //        await _userManager.UpdateAsync(user);
        //    }

        //    return Ok(new { message = "Upload de foto para a base de dados bem-sucedido." });
        //}

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

    public class EditUserModel
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public DateTime? BirthDate { get; set; }
        public int? Age { get; set; }
        public string? Occupation { get; set; }

        public string? Nationality { get; set; }

        public string? ProfilePicture { get; set; }

        public string? Gender { get; set; }

        //public byte[]? ProfilePictureBinary { get; set; }
    }
}
