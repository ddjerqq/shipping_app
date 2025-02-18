using Microsoft.JSInterop;

namespace Presentation.Services;

public sealed class FileDownloader(IJSRuntime js) : IAsyncDisposable
{
    private readonly Lazy<Task<IJSObjectReference>>? _moduleTask = new(async () =>
        await js.InvokeAsync<IJSObjectReference>("import", "/scripts/FileDownloader.js"));

    public async Task DownloadFileAsync(string fileName, MemoryStream content, CancellationToken ct = default)
    {
        var module = await _moduleTask?.Value!;
        var encodedPayload = Convert.ToBase64String(content.ToArray());
        await module.InvokeVoidAsync("saveAsFile", ct, fileName, encodedPayload);
    }

    public async ValueTask DisposeAsync()
    {
        if (_moduleTask is not null)
        {
            try
            {
                var module = await _moduleTask.Value;
                await module.DisposeAsync();
            }
            catch (JSDisconnectedException)
            {
            }
        }
    }
}