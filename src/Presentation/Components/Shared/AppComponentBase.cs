using Blazored.Toast.Configuration;
using Blazored.Toast.Services;
using MediatR;
using Microsoft.AspNetCore.Components;
using TailwindMerge;

namespace Presentation.Components.Shared;

public abstract class AppComponentBase : ComponentBase, IDisposable
{
    private CancellationTokenSource? _cancellationTokenSource;

    [Inject]
    protected IMediator Mediator { get; set; } = null!;

    [Inject]
    protected TwMerge Tw { get; set; } = null!;

    [Inject]
    protected NavigationManager NavigationManager { get; set; } = null!;

    [Inject]
    protected IToastService Toast { get; set; } = null!;

    protected CancellationToken CancellationToken => (_cancellationTokenSource ??= new CancellationTokenSource()).Token;

    protected bool IsLoading { get; set; }

    protected async Task<TResponse> SendCommandAsync<TResponse>(IRequest<TResponse> request)
    {
        try
        {
            IsLoading = true;
            return await Mediator.Send(request, CancellationToken);
        }
        finally
        {
            IsLoading = false;
        }
    }

    protected async Task SendCommandAsync(IRequest request)
    {
        try
        {
            IsLoading = true;
            await Mediator.Send(request, CancellationToken);
        }
        finally
        {
            IsLoading = false;
        }
    }

    protected void ShowSuccess(string message, Action<ToastSettings>? settings = null) => Toast.ShowSuccess(message, settings);
    protected void ShowInfo(string message, Action<ToastSettings>? settings = null) => Toast.ShowInfo(message, settings);
    protected void ShowWarning(string message, Action<ToastSettings>? settings = null) => Toast.ShowWarning(message, settings);
    protected void ShowError(string message, Action<ToastSettings>? settings = null) => Toast.ShowError(message, settings);

    /// <inheritdoc />
    void IDisposable.Dispose()
    {
        GC.SuppressFinalize(this);

        if (_cancellationTokenSource is null)
            return;

        _cancellationTokenSource.Cancel();
        _cancellationTokenSource.Dispose();
        _cancellationTokenSource = null;
    }
}