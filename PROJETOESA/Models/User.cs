using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.Security.Permissions;

namespace PROJETOESA.Models
{
    public class User : IdentityUser
    {
        [Key]
        public Guid UserId { get; set; }
    }
}
