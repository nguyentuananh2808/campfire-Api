using System.Collections.Concurrent;

namespace Campfire.Modules.Werewolf.Infrastructure.Realtime;

public sealed class WerewolfConnectionRegistry
{
    private readonly ConcurrentDictionary<(Guid RoomId, Guid UserId), ConcurrentDictionary<string, byte>> connections = new();
    public bool Add(Guid roomId, Guid userId, string connectionId)
    {
        var set = connections.GetOrAdd((roomId, userId), _ => new ConcurrentDictionary<string, byte>());
        set[connectionId] = 0;
        return set.Count == 1;
    }
    public bool Remove(Guid roomId, Guid userId, string connectionId)
    {
        if (!connections.TryGetValue((roomId, userId), out var set)) return false;
        set.TryRemove(connectionId, out _);
        if (!set.IsEmpty) return false;
        connections.TryRemove((roomId, userId), out _);
        return true;
    }
}
