using Mono.TextTemplating;
using System.ComponentModel.DataAnnotations;

namespace PROJETOESA.Models
{
    public class Carrier
    {
        [Key]
        public string Id { get; set; }
        public string Name { get; set; }
        public string LogoURL { get; set; }
        public int SearchTimes { get; set; }

        public List<Segment>? Segments { get; set; }
    }
}
