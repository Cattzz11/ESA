namespace PROJETOESA.Models.SquareModels
{
    public class SquareCard
    {
        public string Id { get; set; }
        public string CardBrand { get; set; }
        public string Last4 { get; set; }
        public long ExpMonth { get; set; }
        public long ExpYear { get; set; }
        public string CardholderName { get; set; }
        public BillingAddress BillingAddress { get; set; }
        public string Fingerprint { get; set; }
    }

    public class BillingAddress
    {
        public string PostalCode { get; set; }
    }
}
