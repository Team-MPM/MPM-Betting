using Microsoft.JSInterop;

namespace MPM_Betting.Blazor.Components;

// This class provides an example of how JavaScript functionality can be wrapped
// in a .NET class for easy consumption. The associated JavaScript module is
// loaded on demand when first needed.
//
// This class can be registered as scoped DI service and then injected into Blazor
// components for use.

public class ExampleJsInterop : IAsyncDisposable
{
    private readonly Lazy<Task<IJSObjectReference>> m_ModuleTask;

    public ExampleJsInterop(IJSRuntime jsRuntime)
    {
        m_ModuleTask = new(() => jsRuntime.InvokeAsync<IJSObjectReference>(
            "import", "./_content/MPM_Betting.Blazor.Components/exampleJsInterop.js").AsTask());
    }

    public async ValueTask<string> Prompt(string message)
    {
        var module = await m_ModuleTask.Value;
        return await module.InvokeAsync<string>("showPrompt", message);
    }

    public async ValueTask DisposeAsync()
    {
        if (m_ModuleTask.IsValueCreated)
        {
            var module = await m_ModuleTask.Value;
            await module.DisposeAsync();
        }
    }
}