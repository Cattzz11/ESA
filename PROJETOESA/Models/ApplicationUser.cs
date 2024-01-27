using Microsoft.AspNetCore.Identity;

namespace PROJETOESA.Models
{
    public class ApplicationUser : IdentityUser
    {
        [PersonalData]
        public string Name { get; set; }
    }
}
