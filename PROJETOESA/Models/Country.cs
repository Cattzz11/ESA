using System.ComponentModel.DataAnnotations;

namespace PROJETOESA.Models
{
    public class Country
    {
        [Key]
        public string Id { get; set; }
        public string Name { get; set; }

        public List<City>? Cities { get; set; }
    }
}
