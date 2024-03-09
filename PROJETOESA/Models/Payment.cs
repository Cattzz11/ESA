using Humanizer.Localisation;
using System.ComponentModel.DataAnnotations;

namespace PROJETOESA.Models
{
    public class Payment
    {
        [Key]
        public Guid PaymentId { get; set; }

        [Required(ErrorMessage ="Required")]
        [DataType(DataType.Currency)]
        public double Price { get; set; }

        public int? Entity { get; set; }

        public int? Reference {  get; set; }

        public DateTime? LimitDate {  get; set; }

        [Required(ErrorMessage = "Required")]
        [EnumDataType(typeof(PaymentStatus))]
        public PaymentStatus paymentStatus { get; set; }

        public string? PaymentMethod {  get; set; }
    }
}
