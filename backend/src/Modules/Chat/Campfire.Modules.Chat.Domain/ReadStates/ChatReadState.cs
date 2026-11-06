namespace Campfire.Modules.Chat.Domain.ReadStates;

public sealed class ChatReadState
{
    private ChatReadState() { }

    private ChatReadState(Guid groupId, Guid userId, DateTimeOffset lastReadAtUtc)
    {
        GroupId = groupId;
        UserId = userId;
        LastReadAtUtc = lastReadAtUtc;
    }

    public Guid GroupId { get; private set; }
    public Guid UserId { get; private set; }
    public DateTimeOffset LastReadAtUtc { get; private set; }

    public static ChatReadState Create(Guid groupId, Guid userId, DateTimeOffset lastReadAtUtc) =>
        new(groupId, userId, lastReadAtUtc);

    public void MarkRead(DateTimeOffset lastReadAtUtc)
    {
        if (lastReadAtUtc > LastReadAtUtc)
        {
            LastReadAtUtc = lastReadAtUtc;
        }
    }
}
