using System.Collections.Concurrent;

namespace ChatR.Server;

public class RoomTracker
{
    private readonly ConcurrentDictionary<string, string> current = new();
    public string? Get(string connectionId)
        => current.TryGetValue(connectionId, out var r) ? r : null;
    public void Set(string connectionId, string room)
        => current[connectionId] = room;
    public void Remove(string connectionId)
        => current.TryRemove(connectionId, out _);
}
