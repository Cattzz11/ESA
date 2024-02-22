using System.ComponentModel.DataAnnotations.Schema;

namespace PROJETOESA.Models
{
    public class Airport
    {
        public string id { get; set; }
        public string name { get; set; }
        public string apiKey { get; set; }
        public string countryId { get; set; }

        public Country country { get; set; }
    }
}



