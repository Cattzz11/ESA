using System.ComponentModel.DataAnnotations.Schema;

namespace PROJETOESA.Models
{
    public class UserFlight
    {
        public string UserId { get; set; }
        public string FlightId { get; set; }


        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }

        [ForeignKey("FlightId")]
        public virtual Flight Flight { get; set; }

        public List<AccompanyingPassenger>? AccompanyingPassengers { get; set; }
    }
}
