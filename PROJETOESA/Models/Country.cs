namespace PROJETOESA.Models
{
    public class Country
    {
        public string id { get; set; }
        public string name { get; set; }

        public List<Airport>? airports { get; set; }
    }
}
