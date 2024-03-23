using Mailjet.Client;
using Mailjet.Client.TransactionalEmails;
using Microsoft.Extensions.Options;
using PROJETOESA.Models;
using System.Diagnostics;
using System.Threading.Tasks;

namespace PROJETOESA.Services.EmailService
{
    public class MailjetEmailSender : IEmailService
    {
        private readonly IOptions<MailjetSettings> _mailjetSettings;

        public MailjetEmailSender(IOptions<MailjetSettings> mailjetSettings)
        {
            _mailjetSettings = mailjetSettings;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            Debug.WriteLine("AQUI");
            var client = new MailjetClient(_mailjetSettings.Value.ApiKey, _mailjetSettings.Value.ApiSecret);

            var email = new TransactionalEmailBuilder()
                .WithFrom(new SendContact("201400314@estudantes.ips.pt", "AeroHelper"))
                .WithSubject(subject)
                .WithHtmlPart(body)
                .WithTo(new SendContact(toEmail))
                .Build();

            await client.SendTransactionalEmailAsync(email);
        }
    }
}
