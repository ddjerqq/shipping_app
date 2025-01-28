namespace Application.Services;

public interface IEmailSender
{
    /// <summary>
    /// Sends an email with the specified subject, content, recipients, and from address.
    /// </summary>
    public Task SendAsync(string recipient, string subject, string content, CancellationToken ct = default);
}