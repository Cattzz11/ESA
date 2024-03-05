using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using PROJETOESA.Models;
using System.Net;
using System.Net.Mail;

public class EmailSender : IEmailSender
{
    private readonly EmailSettings _emailSettings;

    public EmailSender(IOptions<EmailSettings> emailSettings)
    {
        _emailSettings = emailSettings.Value;
    }

    public async Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        var mailMessage = new MailMessage
        {
            From = new MailAddress(_emailSettings.Sender, _emailSettings.SenderName),
            Subject = subject,
            Body = htmlMessage,
            IsBodyHtml = true
        };

        mailMessage.To.Add(new MailAddress(email));

        using var client = new SmtpClient(_emailSettings.MailServer, _emailSettings.MailPort)
        {
            Credentials = new NetworkCredential(_emailSettings.Sender, _emailSettings.Password),
            EnableSsl = _emailSettings.EnableSSL
        };

        await client.SendMailAsync(mailMessage);
    }
}