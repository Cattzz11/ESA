using Microsoft.AspNetCore.Identity;

namespace PROJETOESA.Models
{
    public class ApplicationUser : IdentityUser
    {
        [PersonalData]
        public string Name { get; set; }
        public TipoConta Role { get; set; }
        public DateTime? BirthDate { get; set; }
        public string? Nationality { get; set; }
        public string? Occupation { get; set; }
        public string? Gender { get; set; }

        public List<UserFlight>? UserFlights { get; set; }
    }
}