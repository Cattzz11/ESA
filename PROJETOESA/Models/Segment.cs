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

        public string OriginCityId { get; set; }
        public string DestinationCityId { get; set; }
        [ForeignKey("OriginCityId")]
        public virtual City OriginCity { get; set; }
        [ForeignKey("DestinationCityId")]
        public virtual City DestinationCity { get; set; }

        public string CarrierId { get; set; }
        [ForeignKey("CarrierId")]
        public virtual Carrier Carrier { get; set; }
        public string FlightId { get; set; }
        [ForeignKey("FlightId")]
        public virtual Flight Flight { get; set; }
    }
}
