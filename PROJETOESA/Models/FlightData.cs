namespace PROJETOESA.Models
{
    public class FlightData
    {
        public string fromEntityId { get; set; }
        public string toEntityId { get; set; }
        public string departDate { get; set; }
        public string? returnDate { get; set; }
        public string? market { get; set; }
        public string? locale { get; set; }
        public string? currency { get; set; }
        public int? Adults { get; set; }
        public int? Children { get; set; }
        public int? Infants { get; set; }
        public string? cabinClass { get; set; }
    }
}
