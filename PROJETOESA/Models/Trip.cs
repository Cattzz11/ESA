using PROJETOESA.Models.ViewModels;
using PROJETOESA.Models;

namespace PROJETOESA.Models
{
    public class Trip
    {
        public string Id { get; set; }
        public double? Price { get; set; }
        public string? SessionId { get; set; }
        public string? Token { get; set; }
        public virtual ICollection<Flight> Flights { get; set; }
    }
}
