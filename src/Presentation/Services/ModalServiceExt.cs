using Blazored.Modal;
using Blazored.Modal.Services;
using Microsoft.AspNetCore.Components;
using Presentation.Components.Shared;

namespace Presentation.Services;

public static class ModalServiceExt
{
    public static IModalReference ShowConfirmationModal(this IModalService modalService, MarkupString text)
    {
        var parameters = new ModalParameters().Add(nameof(ConfirmationModal.Text), text);
        return modalService.Show<ConfirmationModal>(parameters);
    }
}