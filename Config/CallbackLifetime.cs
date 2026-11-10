namespace Game_Manager_GoldKingZ;
public sealed class CallbackLifetime
{
    private long generation;
    public long Capture() => Interlocked.Read(ref generation);
    public bool IsCurrent(long captured) => Capture() == captured;
    public void Invalidate() => Interlocked.Increment(ref generation);
}
