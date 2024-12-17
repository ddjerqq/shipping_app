using System.Net;
using System.Net.Mail;
using Application.Services;
using Domain.Aggregates;
using Domain.Common;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

public sealed class GoogleMailSender : IEmailSender
{
    private readonly SmtpClient _client;
    private readonly ILogger<GoogleMailSender> _logger;

    public GoogleMailSender(ILogger<GoogleMailSender> logger)
    {
        _logger = logger;

        var username = "SMTP__USERNAME".FromEnvRequired();
        var password = "SMTP__PASSWORD".FromEnvRequired();
        var port = int.Parse("SMTP__PORT".FromEnvRequired());
        var host = "SMTP__HOST".FromEnvRequired();

        _client = new SmtpClient
        {
            Host = host,
            Port = port,
            EnableSsl = true,
            UseDefaultCredentials = false,
            Credentials = new NetworkCredential(username, password),
            DeliveryMethod = SmtpDeliveryMethod.Network,
            Timeout = 20_000,
        };
    }

    public async Task SendAsync(string from, string recipient, string subject, string body, CancellationToken ct = default)
    {
        var fromAddress = new MailAddress(from);
        var toAddress = new MailAddress(recipient);

        using var msg = new MailMessage(fromAddress, toAddress);
        msg.IsBodyHtml = false;
        msg.Subject = subject;
        msg.Body = body;
        msg.IsBodyHtml = true;

#if DEBUG
        _logger.LogInformation("sending message to {recipient} {message}", recipient, msg);
#else
        try
        {
            await _client.SendMailAsync(msg, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email");
            throw;
        }
#endif
    }

    public Task SendEmailConfirmationAsync(User recipient, string callback, CancellationToken ct = default) =>
        SendAsync("noreply@localhost", recipient.Email, "Confirm your email", $"Please confirm your account by clicking this link: {callback}", ct);

    public Task SendPasswordResetAsync(User user, string callback, CancellationToken ct = default) =>
        SendAsync("noreply@localhost", user.Email, "Reset your password", $"Please reset your password by clicking this link: {callback}", ct);
}