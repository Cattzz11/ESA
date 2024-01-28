using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Identity.UI.Services;
using MimeKit;
using MimeKit.Text;


public class EmailSender : IEmailSender
{
    private readonly string host;
    private readonly int port;
    private readonly bool enableSSL;
    private readonly string userName;
    private readonly string password;
    private readonly SmtpClient smtpClient;

    public EmailSender(string host, int port, bool enableSSL, string userName, string password)
    {
        this.host = host;
        this.port = port;
        this.enableSSL = enableSSL;
        this.userName = userName;
        this.password = password;

        // Create and configure SmtpClient
        this.smtpClient = new SmtpClient();
        this.smtpClient.ServerCertificateValidationCallback = (s, c, h, e) => true;
    }

    public async Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Aerohelper", "aerohelper2024@outlook.com"));
            message.To.Add(new MailboxAddress(userName, email));
            message.Subject = subject;
            message.Body = new TextPart(TextFormat.Html)
            {
                Text = htmlMessage
            };

            using (var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(60))) // Set a timeout
            {
                await smtpClient.ConnectAsync(host, port, enableSSL, cancellationTokenSource.Token);
                await smtpClient.AuthenticateAsync(userName, password, cancellationTokenSource.Token);
                await smtpClient.SendAsync(message, cancellationTokenSource.Token);
                await smtpClient.DisconnectAsync(true, cancellationTokenSource.Token);
            }
        }
        catch (Exception ex)
        {
            // Handle or log the exception
            Console.WriteLine($"Error sending email: {ex.Message}");
        }
    }
}