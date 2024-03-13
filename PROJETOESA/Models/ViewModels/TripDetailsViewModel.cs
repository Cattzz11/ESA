using System.Collections.Specialized;

namespace PROJETOESA.Models.ViewModels
{
    public class TripDetailsViewModel
    {
        public string Id { get; set; }
        public string DestinationImage {  get; set; }
        public List<FlightViewModel> Flights { get; set; }
        public List<PriceOptions> PriceOptions { get; set; }
    }
}
