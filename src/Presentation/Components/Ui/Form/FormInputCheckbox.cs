using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Rendering;
using Presentation.Common;
using TailwindMerge;

namespace Presentation.Components.Ui.Form;

public sealed class FormInputCheckbox : InputBase<bool>
{
    public const string BaseClass =
        "peer h-4 w-4 shrink-0 rounded-sm border border-primary ring-offset-background focus-visible:outline-none " +
        "focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 disabled:cursor-not-allowed " +
        "disabled:opacity-50 data-[state=checked]:bg-primary data-[state=checked]:text-primary-foreground";

    [Inject]
    public TwMerge Tw { get; set; } = default!;

    [Parameter]
    public string Class { get; set; } = default!;

    [DisallowNull]
    public ElementReference? Element { get; private set; }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "input");
        builder.AddMultipleAttributes(1, AdditionalAttributes);
        builder.AddAttribute(2, "role", "checkbox");
        builder.AddAttribute(3, "type", "checkbox");
        builder.AddAttributeIfNotNullOrWhiteSpace(4, "name", NameAttributeValue);
        builder.AddAttributeIfNotNullOrWhiteSpace(5, "class", Tw.Merge(BaseClass, CssClass, Class));
        builder.AddAttribute(6, "value", CurrentValueAsString);
        builder.AddAttribute(7, "onchange", EventCallback.Factory.CreateBinder<string?>(this, value => CurrentValueAsString = value, CurrentValueAsString));
        builder.SetUpdatesAttributeName("value");
        builder.AddElementReferenceCapture(8, inputReference => Element = inputReference);
        builder.CloseElement();
    }


    protected override bool TryParseValueFromString(string? value, out bool result, [NotNullWhen(false)] out string? validationErrorMessage)
    {
        result = value == "on";
        validationErrorMessage = null;
        return true;
    }
}