using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Components;
using Presentation.Components.Shared;

namespace Presentation.Components.Ui;

public abstract class UiComponentBase : AppComponentBase
{
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object> AdditionalAttributes { get; set; } = [];

    protected string? Class => AdditionalAttributes.GetValueOrDefault("class") as string;

    /// <summary>
    /// Merges tailwind classes, automatically applying the component prop class
    /// </summary>
    protected string? Merge([StringSyntax("html")] params string?[] classes) => Tw.Merge(classes.Append(Class).ToArray());
}