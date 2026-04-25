using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;

namespace server.Services;

public interface IEmailService
{
    Task SendEmailAsync(string toEmail, string toName, string subject, string bodyHtml, CancellationToken ct = default);
}

public class EmailService(IConfiguration configuration) : IEmailService
{
    public async Task SendEmailAsync(string toEmail, string toName, string subject, string bodyHtml, CancellationToken ct = default)
    {
        var host = configuration["Smtp:Host"];
        if (string.IsNullOrWhiteSpace(host))
        {
            // Skip sending if SMTP is not configured
            return;
        }

        var port = configuration.GetValue<int>("Smtp:Port");
        var username = configuration["Smtp:Username"];
        var password = configuration["Smtp:Password"];
        var fromEmail = configuration["Smtp:FromEmail"] ?? "no-reply@busmanagement.com";
        var fromName = configuration["Smtp:FromName"] ?? "Bus Management System";

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(fromName, fromEmail));
        message.To.Add(new MailboxAddress(toName, toEmail));
        message.Subject = subject;

        var bodyBuilder = new BodyBuilder
        {
            HtmlBody = bodyHtml,
            TextBody = "Please view this email in an HTML compatible mail client."
        };

        message.Body = bodyBuilder.ToMessageBody();

        using var client = new SmtpClient();
        
        await client.ConnectAsync(host, port, SecureSocketOptions.StartTls, ct);
        if (!string.IsNullOrWhiteSpace(username) && !string.IsNullOrWhiteSpace(password))
        {
            await client.AuthenticateAsync(username, password, ct);
        }

        await client.SendAsync(message, ct);
        await client.DisconnectAsync(true, ct);
    }
}
