namespace PROJETOESA.Services.EasyPay.Models
{
    public class SinglePaymentDetails
    {
        public Guid Id { get; set; }
        public CustomerData? Customer { get; set; }
        public double Value { get; set; }
        public MethodData? Method { get; set; }
        public string? PaymentStatus { get; set; }
        public DateTime PaidAt { get; set; }

    }
}
