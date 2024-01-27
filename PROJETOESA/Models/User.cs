using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PROJETOESA.Models
{
    public class User: IdentityUser
    {
        [Required]
        [EmailAddress]
        public String Email { get; set; }
        
        [Required]
        [DataType(DataType.Password)]
        public String Password { get; set; }

        [MaxLength(150)]
        [Required(ErrorMessage = "O nome é obrigatório")]
        [Display(Name = "Nome")]
        public String Name { get; set; }

        [Required(ErrorMessage = "A data é obrigatória")]
        [Display(Name = "Data de Nascimento")]
        [DataType(DataType.Date)]
        public DateTime BirthDate { get; set; }

        [MaxLength(150)]
        [Display(Name = "Ocupação")]
        public String Occupation { get; set; }

        [MaxLength(150)]
        [Display(Name = "Nacionalidade")]
        public String Nationality { get; set; }

        [MaxLength(150)]
        [Display(Name = "Imagem")]
        public String PhotoId { get; set; }


    }
}
