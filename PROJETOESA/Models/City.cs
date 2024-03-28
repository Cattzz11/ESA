using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PROJETOESA.Models
{
    public class City
    {
        [Key]
        public string Id { get; set; }
        public string Name { get; set; }
        public string ApiKey { get; set; }

        public string CountryId { get; set; }
        public string? Coordenates { get; set; }
        public virtual Country Country { get; set; }
    }
}
