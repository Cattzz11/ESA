using System.ComponentModel.DataAnnotations.Schema;

namespace PROJETOESA.Models
{
    public class LoginModel
    {
        //[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string UserId { get; set; }
        public DateTime LoginTime { get; set; }
    }
}
