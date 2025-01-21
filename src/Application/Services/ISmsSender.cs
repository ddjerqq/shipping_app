namespace Application.Services;

public interface ISmsSender
{
    /// <summary>
    /// Sends an SMS message with the specified content to the specified number.
    /// </summary>
    /// <param name="number">phone number in RFC</param>
    /// <param name="content">the content to send</param>
    /// <param name="ct"></param>
    public Task SendAsync(string number, string content, CancellationToken ct = default);
}