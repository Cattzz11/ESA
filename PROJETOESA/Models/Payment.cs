using Humanizer.Localisation;
using System.ComponentModel.DataAnnotations;

namespace PROJETOESA.Models
{
    public class Payment
    {
        [Key]
        public string PaymentId { get; set; }

        public string CustomerId {  get; set; }

        public DateTime date { get; set; }

        public string paymentState { get; set; }
    }
}
