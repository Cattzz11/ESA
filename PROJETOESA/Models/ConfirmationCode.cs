﻿namespace PROJETOESA.Models
{
    public class ConfirmationCode
    {
        public int Id { get; set; }
        public string UserEmail { get; set; }
        public string Code { get; set; }
        public DateTime ExpirationTime { get; set; }
    }

}
