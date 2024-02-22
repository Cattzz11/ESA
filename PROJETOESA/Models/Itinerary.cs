using System.Security.Cryptography;

namespace PROJETOESA.Models
{
    public class Itinerary
    {
        public string id { get; set; }
        public double price { get; set; }
        public List<Trip> trip { get; set; }
        public bool isSelfTransfer { get; set; }
        public bool isProtectedSelfTransfer { get; set; }
        public bool isChangeAllowed { get; set; }
        public bool isPartiallyChangeable { get; set; }
        public bool isCancellationAllowed { get; set; }
        public bool isPartiallyRefundable { get; set; }
        public int score { get; set; }
    }
}
