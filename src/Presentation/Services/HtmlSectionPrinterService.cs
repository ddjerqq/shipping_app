using Microsoft.JSInterop;

namespace Presentation.Services;

public sealed class HtmlSectionPrinterService(IJSRuntime js) : IAsyncDisposable
{
    private readonly Lazy<Task<IJSObjectReference>>? _moduleTask = new(async () =>
        await js.InvokeAsync<IJSObjectReference>("import", "/scripts/HtmlSectionPrinterService.js"));

    public async Task Print(string selector, CancellationToken ct = default)
    {
        var module = await _moduleTask?.Value!;
        await module.InvokeVoidAsync("print", ct, selector);
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