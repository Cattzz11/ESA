using Microsoft.EntityFrameworkCore;
using PROJETOESA.Data;
using PROJETOESA.Models;

namespace PROJETOESA.Services
{
    public class StatisticsService
    {
        private readonly AeroHelperContext _context; // Substitua YourDbContext pelo nome do seu contexto de dados
        //private readonly ApplicationUser _user;

        public StatisticsService(AeroHelperContext context)
        {
            _context = context;
            //_user = user;
        }

        public async Task<Statistics> GetStatisticsAsync()
        {
            var totalUsers =_context.Users.Count() ; // Assumindo que você tem uma tabela de Users

            return new Statistics
            {
                totalUsersStats = totalUsers
            };
        }

        public async Task<int> GetLoginsByDateAsync(DateTime date)
        {
            var dateStart = date.Date; // Meia-noite do dia
            var dateEnd = dateStart.AddDays(1); // Meia-noite do dia seguinte

            return await _context.Logins
                                 .Where(login => login.LoginTime >= dateStart && login.LoginTime < dateEnd)
                                 .Select(login => login.UserId)
                                 .Distinct() // Para contar cada usuário apenas uma vez
                                 .CountAsync();
        }



    }
}
