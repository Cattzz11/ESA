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
            var totalUsers =_context.Users.Count(u => u.Role.Equals(TipoConta.ClienteNormal)) ; // Assumindo que você tem uma tabela de Users
            var premiumUsers = _context.Users.Count(u => u.Role.Equals(TipoConta.ClientePremium));

            return new Statistics
            {
                TotalUsersStats = totalUsers,
                TotalPremiumStats = premiumUsers
            };
        }

        public async Task<int> GetLoginsByDateAsync(DateTime date)
        {
            var dateStart = date.Date; // Meia-noite do dia
            var dateEnd = dateStart.AddDays(1); // Meia-noite do dia seguinte

            Console.WriteLine(dateStart);
            Console.WriteLine(dateEnd);

            return await _context.Logins
                                 .Where(login => login.LoginTime >= dateStart && login.LoginTime < dateEnd)
                                 .Select(login => login.UserId)
                                 .Distinct() // Para contar cada usuário apenas uma vez
                                 .CountAsync();
        }

        public async Task<int> GetMaxDailyLoginsAsync()
        {
            var dailyLoginCounts = await _context.Logins
                .GroupBy(l => l.LoginTime.Date)
                .Select(group => new { LoginDate = group.Key, Count = group.Count() })
                .ToListAsync();

            var maxLogins = dailyLoginCounts.Max(l => l.Count);

            return maxLogins;
        }

        public async Task<int> GetMaxDailyRegistrationsAsync()
        {
            // Agrupa os registros por data, ignorando a parte do tempo
            var dailyRegistrations = from user in _context.Users
                                     group user by user.registerTime.Date into dateGroup
                                     select new
                                     {
                                         Date = dateGroup.Key,
                                         Count = dateGroup.Count()
                                     };

            // Busca o máximo de registros em um único dia
            var maxDailyRegistrations = await dailyRegistrations.MaxAsync(x => x.Count);

            return maxDailyRegistrations;
        }
    }
}
