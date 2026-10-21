using Campfire.Modules.Groups.Domain.Auditing;
using Campfire.Modules.Groups.Domain.Groups;
using Campfire.Modules.Groups.Domain.Invitations;
using Microsoft.EntityFrameworkCore;

namespace Campfire.Modules.Groups.Infrastructure.Persistence;

public sealed class GroupsDbContext(DbContextOptions<GroupsDbContext> options) : DbContext(options)
{
    public const string Schema = "groups";

    public DbSet<Group> Groups => Set<Group>();

    public DbSet<GroupMember> Memberships => Set<GroupMember>();

    public DbSet<GroupInvitation> Invitations => Set<GroupInvitation>();

    public DbSet<GroupAuditRecord> AuditRecords => Set<GroupAuditRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(GroupsDbContext).Assembly);
    }
}
