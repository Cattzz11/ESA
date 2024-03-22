namespace PROJETOESA.Services.EasyPayService.Models
{
    public class RequestData
    {
        public DateTime ExpirationTime { get; set; }
        public CustomerData? Customer { get; set; }
        public double Value { get; set; }
        public string? Method { get; set; }
    }
}
