namespace MPM_Betting.Blazor.ComponentLibrary.Core;

public class SyncContextWithPayload<T>
{
    public ManualResetEventSlim SyncEvent { get; } = new();
    public T? Payload { get; set; }
}