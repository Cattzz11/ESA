namespace PROJETOESA.Models.ViewModels
{
    public class SegmentViewModel
    {
        public string FlightNumber { get; set; }
        public DateTime Departure { get; set; }
        public DateTime Arrival { get; set; }
        public string Duration { get; set; }
        public CityViewModel OriginCity { get; set; }
        public CityViewModel DestinationCity { get; set; }
        public CarrierViewModel? Carrier { get; set; }
    }
}
