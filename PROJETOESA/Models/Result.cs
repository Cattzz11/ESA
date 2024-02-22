namespace PROJETOESA.Models
{
    public class Result
    {
        public string Id { get; set; }
        public string CodeLocation { get; set; } // location -> skyCode
        public string Location { get; set; } // location -> name
        public List<FlightQuotes> FlightQuotes { get; set; }
        public string ImageUrl { get; set; }
    }
}
