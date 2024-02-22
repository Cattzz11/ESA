using System.Threading;

namespace PROJETOESA.Models
{
    public class Trip
    {
        public string id { get; set; }
        public Place origin { get; set; }
        public Place destination { get; set; }
        public string durationInMinutes { get; set; }
        public int stopCount { get; set;}
        public DateTime departure { get; set; }
        public DateTime arrival { get; set; }
        public int timeDeltaInDays { get; set; }
        public List<Carrier> carriers { get; set; }
        public List<Segment> segments { get; set; }
    }
}
