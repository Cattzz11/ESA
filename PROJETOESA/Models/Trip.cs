namespace PROJETOESA.Models
{
    public class Trip
    {
        public string Id { get; set; }
        public double Price { get; set; }
        public bool isSelfTransfer { get; set; }
        public bool isProtectedSelfTransfer { get; set; }
        public bool isChangeAllowed { get; set; }
        public bool isPartiallyChangeable { get; set; }
        public bool isCancellationAllowed { get; set; }
        public bool isPartiallyRefundable { get; set; }
        public double Score { get; set; }
        public virtual ICollection<Flight> Flights { get; set; }
    }
}
