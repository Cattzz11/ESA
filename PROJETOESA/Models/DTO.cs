using System.ComponentModel.DataAnnotations;

namespace PROJETOESA.Models
{
    public class TripDto
    {
        public string Id { get; set; }
        public double Price { get; set; }
        public List<FlightDto> Flights { get; set; } = new List<FlightDto>();
    }

    public class FlightDto
    {
        public string Id { get; set; }
        public string Duration { get; set; }
        public List<SegmentDto> Segments { get; set; } = new List<SegmentDto>();
    }

    public class SegmentDto
    {
        public string FlightNumber { get; set; }
        public DateTime Departure { get; set; }
        public DateTime Arrival { get; set; }
        public string Duration { get; set; }
        public CityDto OriginCity { get; set; }
        public CityDto DestinationCity { get; set; }
        public CarrierDto? Carrier { get; set; }
    }

    public class CarrierDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string LogoURL { get; set; }
        public int SearchTimes { get; set; }
    }

    public class CityDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public CountryDto Country { get; set; }
    }

    public class CountryDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }
}
