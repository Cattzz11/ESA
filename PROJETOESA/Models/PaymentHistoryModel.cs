namespace PROJETOESA.Models
{
    public class PaymentHistoryModel
    {
        public decimal price { get; set; }
        public required string currency { get; set; }
        public string Email { get; set; }
        public string CreditCard { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string ShippingAddress { get; set; }
        public string Status { get; set; }
    }
}
