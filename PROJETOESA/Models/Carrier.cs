namespace PROJETOESA.Models
{
    public class Carrier
    {
        public int id {  get; set; }
        public string? logoUrl { get; set; }
        public string name { get; set; }
        public int searchTimes { get; set; } = 0;
    }
}
