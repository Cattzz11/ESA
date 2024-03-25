namespace PROJETOESA.Services.CodeGeneratorService
{
    public class CodeGeneratorService : ICodeGeneratorService
    {
        public string GenerateCode()
        {
            var random = new Random();
            return random.Next(0, 1000000).ToString("D6");
        }
    }
}
