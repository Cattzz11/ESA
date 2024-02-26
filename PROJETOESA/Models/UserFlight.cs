using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Cryptography.Xml;

namespace PROJETOESA.Models
{
    public class UserFlight
    {
        public string UserId { get; set; }
        public string TripId { get; set; }

        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }

        [ForeignKey("TripId")]
        public virtual Trip Trip { get; set; }

        public virtual ICollection<AccompanyingPassenger> AccompanyingPassengers { get; set; }
    }
}
