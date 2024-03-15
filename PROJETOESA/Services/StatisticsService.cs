using PROJETOESA.Data;
using PROJETOESA.Models;

namespace PROJETOESA.Services
{
    public class StatisticsService
    {
        private readonly AeroHelperContext _context; // Substitua YourDbContext pelo nome do seu contexto de dados

        public StatisticsService(AeroHelperContext context)
        {
            _context = context;
        }

        public async Task<Statistics> GetStatisticsAsync()
        {
            var totalUsers =_context.Users.Count() ; // Assumindo que você tem uma tabela de Users

            return new Statistics
            {
                totalUsersStats = totalUsers
            };
        }
    }
}
