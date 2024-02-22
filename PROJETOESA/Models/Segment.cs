namespace PROJETOESA.Models
{
    public class Segment
    {
        public Place origin { get; set; }
        public Place destination { get; set; }
        public DateTime departure { get; set; }
        public DateTime arrival { get; set; }
        public int durationInMinutes { get; set; }
        public string flightNumber { get; set; }
        public Carrier carrier { get; set; }
    }
}
