using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace PROJETOESA.Models
{
    public class AccompanyingPassenger
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Gender { get; set; }
        public string Nationality { get; set; }
        public DateTime BirthDate { get; set; }

        public string UserId { get; set; }
        public string FlightId { get; set; }

        [ForeignKey("UserId, FlightId")]
        public virtual UserFlight UserFlight { get; set; }
    }
}
