using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace PROJETOESA.Models
{
    public class LoginExterno
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        public ClaimsPrincipal Principal { get; set; }
    }
}
