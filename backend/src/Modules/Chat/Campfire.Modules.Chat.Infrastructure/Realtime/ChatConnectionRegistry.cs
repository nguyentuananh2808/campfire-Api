using System.Collections.Concurrent;

namespace Campfire.Modules.Chat.Infrastructure.Realtime;

public sealed class ChatConnectionRegistry
{
    private readonly ConcurrentDictionary<string, ConcurrentDictionary<(Guid UserId, Guid GroupId), byte>> byConnection = new();

    public void Add(string connectionId, Guid userId, Guid groupId) =>
        byConnection.GetOrAdd(connectionId, _ => new ConcurrentDictionary<(Guid, Guid), byte>())[(userId, groupId)] = 0;

    public IReadOnlyList<string> Find(Guid userId, Guid groupId) => byConnection
        .Where(pair => pair.Value.ContainsKey((userId, groupId)))
        .Select(pair => pair.Key).ToArray();

    public void RemoveConnection(string connectionId) => byConnection.TryRemove(connectionId, out _);

    public void Remove(string connectionId, Guid userId, Guid groupId)
    {
        if (!byConnection.TryGetValue(connectionId, out var subscriptions)) return;
        subscriptions.TryRemove((userId, groupId), out _);
        if (subscriptions.IsEmpty) byConnection.TryRemove(connectionId, out _);
    }
}
