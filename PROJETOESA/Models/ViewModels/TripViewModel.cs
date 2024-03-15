namespace PROJETOESA.Models.ViewModels
{
    public class TripViewModel
    {
        public string Id { get; set; }
        public double Price { get; set; }
        public List<FlightViewModel> Flights { get; set; } = new List<FlightViewModel>();
    }
}
