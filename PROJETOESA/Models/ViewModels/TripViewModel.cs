namespace PROJETOESA.Models.ViewModels
{
    public class TripViewModel
    {
        public string Id { get; set; }
        public double? Price { get; set; }
        public string? SessionId { get; set; }
        public string? Token { get; set; }
        public List<FlightViewModel> Flights { get; set; } = new List<FlightViewModel>();
        public virtual ICollection<PriceOptions>? PriceOptions { get; set; }
    }
}
