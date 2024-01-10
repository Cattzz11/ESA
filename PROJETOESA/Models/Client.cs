using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PROJETOESA.Models
{
    public class Client
    {
        [Key, ForeignKey("User")]
        public Guid ClientId { get; set; }

        [MaxLength(150)]
        [Required(ErrorMessage = "O nome é obrigatório")]
        [Display(Name = "Nome")]
        public String Name { get; set; }

        [Required(ErrorMessage = "A data é obrigatória")]
        [Display(Name = "Data")]
        [DataType(DataType.Date)]
        public DateTime BirthDate { get; set; }

        [MaxLength(150)]
        [Display(Name = "Ocupação")]
        public String Occupation { get; set; }

        [MaxLength(150)]
        [Display(Name = "Nacionalidade")]
        public String Nationality { get; set; }

        [Display(Name = "Subscrito")]
        public bool isSubscribed  { get; set; }

        [MaxLength(150)]
        [Display(Name = "Imagem")]
        public String PhotoId { get; set; }

        [Required]
        [Display(Name = "Utilizador")]
        public User User { get; set; }
    }
}
