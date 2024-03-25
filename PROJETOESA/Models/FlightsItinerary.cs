namespace PROJETOESA.Models
{
    public class FlightsItinerary
    {
        public string FlightIATA { get; set; }
        public string FlightICAO { get; set; }
        public string FlightStatus { get; set; }
        public City DepartureLocation { get; set; }
        public DateTime DepartureSchedule {  get; set; }
        public City ArrivalLocation { get; set; }
        public DateTime ArrivalSchedule { get; set; }
        public Carrier? Airline { get; set; }
    }
}
