﻿namespace PROJETOESA.Services
{
    public class CodeGeneratorService
    {
        public string GenerateCode()
        {
            var random = new Random();
            return random.Next(0, 1000000).ToString("D6");
        }
    }
}
