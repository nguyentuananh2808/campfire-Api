namespace Campfire.Modules.Identity.Contracts;

public interface IUserProfileReader
{
    Task<IReadOnlyDictionary<Guid, UserProfile>> GetByIdsAsync(
        IReadOnlyCollection<Guid> userIds,
        CancellationToken cancellationToken);
}
