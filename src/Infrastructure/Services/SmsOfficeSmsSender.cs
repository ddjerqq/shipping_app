using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Application.Services;
using Domain.Common;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

public sealed class SmsOfficeApiResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = null!;
    public object? Output { get; set; }
    public int ErrorCode { get; set; }

    private static readonly Dictionary<int, string> ErrorCodes = new()
    {
        { 0, "მესიჯი მიღებულია smsoffice -ს მიერ სამომავლოდ ნომერთან გადასაგზავნად. ეს ჯერ არ ნიშნავს, რომ მესიჯი მივიდა მობილურ ტელეფონში. მესიჯის მისვლას შეიტყობთ მიღების უწყისში" },
        { 10, "destination შეიცავს არაქართულ ნომრებს" },
        { 20, "ბალანსი არასაკმარისია" },
        { 40, "გასაგზავნი ტექსტი 160 სიმბოლოზე მეტია" },
        { 60, "ბრძანებას აკლია content პარამეტრის მნიშვნელობა, გასაგზავნი ტექსტი" },
        { 70, "ბრძანებას აკლია ნომრები" },
        { 75, "ყველა ნომერი სტოპ სიაშია" },
        { 76, "ყველა ნომერი არასწორი ფორმატითაა მოწოდებული" },
        { 77, "ყველა ნომერი სტოპ სიაშია ან არასწორი ფორმატითაა მოწოდებული" },
        { 80, "key -ს შესაბამისი მომხმარებელი ვერ მოიძებნა" },
        { 110, "sender პარამეტრის მნიშვნელობა გაუგებარია" },
        { 120, "გააქტიურეთ api -ის გამოყენების უფლება პროფილის გვერდზე" },
        { 150, "sender არ იძებნება სისტემაში. შეამოწმეთ მართლწერა" },
        { 500, "ბრძანებას აკლია key პარამეტრი" },
        { 600, "ბრძანებას აკლია destination პარამეტრი" },
        { 700, "ბრძანებას აკლია sender პარამეტრი" },
        { 800, "ბრძანებას აკლია content პარამეტრი" },
        { -100, "დროებითი შეფერხება" },
    };

    [JsonIgnore]
    public string? ErrorDescription => ErrorCodes.TryGetValue(ErrorCode, out var value) ? value : null;
}

public sealed class SmsOfficeSmsSender(ILogger<SmsOfficeSmsSender> logger, HttpClient http) : ISmsSender
{
    private static string ApiKey => "SMS_OFFICE__API_KEY".FromEnvRequired();

    private static string GetFormattedUrl(string number, string content)
    {
        var cleanNumber = number.TrimStart('+').Replace(" ", string.Empty);
        return $"https://smsoffice.ge/api/v2/send/?key={ApiKey}&destination={cleanNumber}&sender=sangoway&content={content}&urgent=true";
    }

    public async Task<int> GetRemainingSmsBalance(CancellationToken ct = default)
    {
        var url = $"https://smsoffice.ge/api/getBalance?key={ApiKey}";
        var response = await http.GetAsync(url, ct);
        var balance = await response.Content.ReadAsStringAsync(ct);
        return int.Parse(balance);
    }

    public async Task SendAsync(string number, string content, CancellationToken ct = default)
    {
        var balance = await GetRemainingSmsBalance(ct);
        if (balance < 10)
        {
            logger.LogError("Sms sender balance less than 10 ({Balance}). Will not send ***{Recipient} {Content}", balance, number[^2..], content);
            return;
        }

        var url = GetFormattedUrl(number, content);
        var response = await http.PostAsync(url, null, ct);
        var apiResponse = await response.Content.ReadFromJsonAsync<SmsOfficeApiResponse>(cancellationToken: ct);

        if (apiResponse is not { Success: true })
        {
            logger.LogError("Error sending text to ***{Number}: {@ApiResponse}", number[^2..], apiResponse);
        }
    }
}