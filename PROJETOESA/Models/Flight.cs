using Mono.TextTemplating;
using System.ComponentModel.DataAnnotations;

namespace PROJETOESA.Models
{
    public class Flight
    {
        [Key]
        public string Id { get; set; }
        public string Duration { get; set; }
        public DateTime Departure { get; set; }
        public DateTime Arrival { get; set; }

        public string OriginCityId { get; set; }
        public string DestinationCityId { get; set; }
        public virtual City OriginCity { get; set; }
        public virtual City DestinationCity { get; set; }
        public virtual ICollection<Segment> Segments { get; set; }
    }
}