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
using Microsoft.IdentityModel.Tokens;

namespace PROJETOESA.Controllers
{
    [ApiController]
    [Produces("application/json")]
    public class CustomAccountController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSender _emailSender;
        private readonly ILogger<CustomAccountController> _logger;
        private readonly ICodeGeneratorService _codeGeneratorService;
        private readonly AeroHelperContext _context;


        public CustomAccountController(UserManager<ApplicationUser> userManager, IEmailSender emailSender, ILogger<CustomAccountController> logger, ICodeGeneratorService codeGeneratorService, AeroHelperContext context)
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
                user.registerTime = DateTime.Now;
                await _context.SaveChangesAsync();
                return Ok();
            }

            return BadRequest(result.Errors);
        }

        [HttpPost]
        [Route("api/login-time")]
        [Authorize]
        public async Task<IActionResult> LoginTime([FromBody] CustomLoginModel model)
        {
            try{
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user != null)
                {
                    // O usuário foi autenticado com sucesso, agora registre o horário de login
                    var loginRecord = new LoginModel
                    {
                        UserId = user.Id,
                        LoginTime = DateTime.Now // Ou utilize DateTime.Now, dependendo de como deseja registrar o horário
                    };
                    _context.Logins.Add(loginRecord);
                    await _context.SaveChangesAsync();

                    // Aqui você pode gerar e retornar um token JWT ou outra forma de confirmação de login, se necessário
                    return Ok(new { message = "Login-time successful" });
                }
                return Unauthorized(new { message = "Login-time failed" });
            }
            catch (Exception ex)
            {

            }

            return Ok();
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
        [Authorize]
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
                var confirmationCode = this._codeGeneratorService.GenerateCode();

                // Construção da mensagem para enviar
                var htmlContent = $"<p>Seu código de recuperação é: {confirmationCode}</p>";

                // Envio do e-mail
                await _emailSender.SendEmailAsync(model.Email, "Recuperação da Conta", htmlContent);

                // Armazenar o código de recuperação na base de dados
                await StoreRecoveryCodeAsync(model, confirmationCode);
            }

            return Ok(new { Message = "Se o e-mail existir, um e-mail de recuperação será enviado." });
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
        [Route("api/validate-recovery-code")]
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


        //Get:
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
        [Authorize]
        public async Task<IActionResult> GetUsers()
        {
            var user = await _userManager.FindByEmailAsync(User.Identity.Name);

            if (user.Role == TipoConta.Administrador)
            {
                var users = await _context.Users.ToListAsync();
                return Ok(users);
            }

            return NotFound();
            
        }

        [HttpDelete("api/users/{email}")]
        [Authorize]
        public async Task<IActionResult> DeleteUser(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
                return NotFound(); // User not found
            }

            var user2 = await _userManager.FindByEmailAsync(User.Identity.Name);

            if (user2.Email != email)
            {
                return NotFound();
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
        [Authorize]
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

        //public string RegisterTime { get; set; }

    }

    public class CustomLoginModel
    {
        public string Email { get; set; }
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

    public class TripDetails
    {
        public string LogoURL { get; set; }
        public DateTime DepartureDate { get; set; }
        public string DepartureTime { get; set; }
        public string OriginCity { get; set; }
        public int Duration { get; set; }
        public DateTime ArrivalDate { get; set; }
        public string ArrivalTime { get; set; }
        public string DestinationCity { get; set; }
        public decimal Price { get; set; }
        // Add other properties as needed
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