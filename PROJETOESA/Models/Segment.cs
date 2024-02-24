using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace PROJETOESA.Models
{
    public class Segment
    {
        [Key]
        public string FlightNumber { get; set; }
        public DateTime Departure { get; set; }
        public DateTime Arrival { get; set; }
        public string Duration { get; set; }

        public string FlightId { get; set; }

        [ForeignKey("FlightId")]
        public virtual Flight Flight { get; set; }
        public string CarrierId { get; set; }
    }
}
