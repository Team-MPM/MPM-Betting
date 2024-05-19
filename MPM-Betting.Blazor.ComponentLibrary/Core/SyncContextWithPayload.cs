namespace MPM_Betting.Blazor.ComponentLibrary.Core;

public class SyncContextWithPayload<T>
{
    public TaskCompletionSource<bool> SyncEvent { get; } = new(false);
    public T? Payload { get; set; }
}