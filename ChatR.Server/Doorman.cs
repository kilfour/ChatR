using System.Collections.Concurrent;

namespace ChatR.Server;

public class Doorman
{
    private readonly ConcurrentDictionary<string, Chatterer> current = new();
    public Chatterer? Get(string name)
        => current.TryGetValue(name, out var r) ? r : null;
    public void Set(string name, Chatterer chatterer)
        => current[name] = chatterer;
    public void Remove(string name)
        => current.TryRemove(name, out _);
}


