namespace Campfire.Modules.Groups.Domain.Invitations;

public sealed class GroupInvitation
{
    private GroupInvitation()
    {
    }

    private GroupInvitation(
        Guid id,
        Guid groupId,
        byte[] tokenHash,
        Guid createdByUserId,
        DateTimeOffset createdAtUtc,
        DateTimeOffset expiresAtUtc)
    {
        Id = id;
        GroupId = groupId;
        TokenHash = tokenHash;
        CreatedByUserId = createdByUserId;
        CreatedAtUtc = createdAtUtc;
        ExpiresAtUtc = expiresAtUtc;
    }

    public Guid Id { get; private set; }

    public Guid GroupId { get; private set; }

    public byte[] TokenHash { get; private set; } = [];

    public Guid CreatedByUserId { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset ExpiresAtUtc { get; private set; }

    public DateTimeOffset? RevokedAtUtc { get; private set; }

    public bool IsAvailable(DateTimeOffset now) => RevokedAtUtc is null && ExpiresAtUtc > now;

    public static GroupInvitation Create(
        Guid groupId,
        byte[] tokenHash,
        Guid createdByUserId,
        DateTimeOffset createdAtUtc,
        DateTimeOffset expiresAtUtc)
    {
        if (tokenHash.Length != 32)
        {
            throw new ArgumentException("Invitation token hash must be 32 bytes.", nameof(tokenHash));
        }

        if (expiresAtUtc <= createdAtUtc)
        {
            throw new ArgumentOutOfRangeException(nameof(expiresAtUtc));
        }

        return new GroupInvitation(
            Guid.NewGuid(),
            groupId,
            tokenHash,
            createdByUserId,
            createdAtUtc,
            expiresAtUtc);
    }

    public bool Revoke(DateTimeOffset revokedAtUtc)
    {
        if (RevokedAtUtc is not null)
        {
            return false;
        }

        RevokedAtUtc = revokedAtUtc;
        return true;
    }
}
