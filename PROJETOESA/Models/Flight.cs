using Mono.TextTemplating;
using System.ComponentModel.DataAnnotations;

namespace PROJETOESA.Models
{
    public class Flight
    {
        [Key]
        public string Id { get; set; }
        public double Price { get; set; }
        public string Duration { get; set; }
        public DateTime Departure { get; set; }
        public DateTime Arrival { get; set; }
        public bool isSelfTransfer { get; set; }
        public bool isProtectedSelfTransfer { get; set; }
        public bool isChangeAllowed { get; set; }
        public bool isPartiallyChangeable { get; set; }
        public bool isCancellationAllowed { get; set; }
        public bool isPartiallyRefundable { get; set; }
        public double Score { get; set; }

        public List<Segment> Segments { get; set; }
        public string OriginCityId { get; set; }
        public virtual City OriginCity { get; set; }
        public string DestinationCityId { get; set; }
        public virtual City DestinationCity { get; set; }
        public List<UserFlight>? UserFlights { get; set; }
    }
}
