namespace PROJETOESA.Models.ViewModels
{
    public class FlightViewModel
    {
        public string Id { get; set; }
        public DateTime Departure { get; set; }
        public DateTime Arrival { get; set; }
        public string Duration { get; set; }
        public CityViewModel OriginCity { get; set; }
        public CityViewModel DestinationCity { get; set; }
        public int? StopCount { get; set; } = 0;
        public List<SegmentViewModel> Segments { get; set; } = new List<SegmentViewModel>();
    }
}
