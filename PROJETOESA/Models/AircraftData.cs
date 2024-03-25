namespace PROJETOESA.Models
{
    public class AircraftData
    {
        public string? Model { get; set; }
        public string? Photo { get; set; }
        public string? Registration { get; set; }
        public bool? IsActive { get; set; }
        public string? Airline { get; set; }
        public string? ICAO { get; set; }
        public string? ModelCode { get; set; }
        public int? SeatsNumber { get; set; }
        public DateTime? RolloutDate { get; set; }
        public DateTime? FirstFlightDate { get; set; }
        public DateTime? RegistrationDate { get; set; }
        public int? EnginesNumber { get; set; }
        public string? EnginesType { get; set; }
        public bool? IsFreighter { get; set; }
        public string? ProductionLine { get; set; }
        public int? Age { get; set; }
        public int? NumRegistrations { get; set; }
    }
}
