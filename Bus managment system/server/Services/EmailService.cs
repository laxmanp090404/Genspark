using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;

namespace server.Services;

public interface IEmailService
{
    Task SendEmailAsync(
        string toEmail,
        string toName,
        string subject,
        string bodyHtml,
        CancellationToken ct = default
    );
}

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendEmailAsync(
        string toEmail,
        string toName,
        string subject,
        string bodyHtml,
        CancellationToken ct = default)
    {
        var host = _configuration["Smtp:Host"];
        var port = _configuration.GetValue<int>("Smtp:Port");
        var username = _configuration["Smtp:Username"];
        var password = _configuration["Smtp:Password"];
        var fromEmail = _configuration["Smtp:FromEmail"] ?? "no-reply@busmanagement.com";
        var fromName = _configuration["Smtp:FromName"] ?? "Bus Management System";

        // 🔴 Validate config early
        if (string.IsNullOrWhiteSpace(host))
        {
            _logger.LogWarning("SMTP Host is not configured. Skipping email.");
            return;
        }

        try
        {
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

            // ✅ IMPORTANT FIX for Mailtrap
            // Use StartTls for port 587
            await client.ConnectAsync(host, port, SecureSocketOptions.StartTls, ct);
            

            // Optional: accept all SSL certs (use only in dev)
            client.ServerCertificateValidationCallback = (s, c, h, e) => true;

            if (!string.IsNullOrWhiteSpace(username) &&
                !string.IsNullOrWhiteSpace(password))
            {
                await client.AuthenticateAsync(username, password, ct);
            }

            await client.SendAsync(message, ct);
            await client.DisconnectAsync(true, ct);

            _logger.LogInformation("Email sent successfully to {Email}", toEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Email}", toEmail);
            throw; // rethrow so you can debug in API response if needed
        }
    }
}