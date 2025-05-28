using System.Net;
using System.Net.Mail;
using Application.Services;
using Domain.Common;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services.Notifications;

public sealed class GoogleMailSender : IEmailSender
{
    private readonly string _username;
    private readonly SmtpClient _client;
    private readonly ILogger<GoogleMailSender> _logger;

    public GoogleMailSender(ILogger<GoogleMailSender> logger)
    {
        _logger = logger;
        _username = "GOOGLE__USERNAME".FromEnvRequired();
        var password = "GOOGLE__APP_PASSWORD".FromEnvRequired();

        _client = new SmtpClient("smtp.gmail.com")
        {
            Port = 587,
            EnableSsl = true,
            UseDefaultCredentials = false,
            Credentials = new NetworkCredential(_username, password),
            DeliveryMethod = SmtpDeliveryMethod.Network,
            Timeout = 10_000,
        };
    }

    public async Task SendAsync(string recipient, string subject, string body, CancellationToken ct = default)
    {
        var fromAddress = new MailAddress(_username);
        var toAddress = new MailAddress(recipient);

        using var msg = new MailMessage(fromAddress, toAddress);
        msg.IsBodyHtml = false;
        msg.Subject = $"noreply - {subject}";
        msg.Body = body;
        msg.IsBodyHtml = true;

        _logger.LogInformation("sending email to {Recipient}", recipient[..2]);

        try
        {
            await Task.Run(() => _client.Send(msg), ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email");
            throw;
        }
    }
}