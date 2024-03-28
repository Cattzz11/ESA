using PROJETOESA.Models;

namespace PROJETOESA.Models.ViewModels
{
    public class FlightViewModel
    {
        public string Id { get; set; }
        public string Duration { get; set; }
        public DateTime Departure { get; set; }
        public DateTime Arrival { get; set; }
        public CityViewModel OriginCity { get; set; }
        public CityViewModel DestinationCity { get; set; }
        public List<SegmentViewModel> Segments { get; set; } = new List<SegmentViewModel>();
    }
}