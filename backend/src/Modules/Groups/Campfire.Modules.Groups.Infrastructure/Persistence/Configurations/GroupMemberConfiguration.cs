using Campfire.Modules.Groups.Domain.Groups;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Campfire.Modules.Groups.Infrastructure.Persistence.Configurations;

internal sealed class GroupMemberConfiguration : IEntityTypeConfiguration<GroupMember>
{
    public void Configure(EntityTypeBuilder<GroupMember> builder)
    {
        builder.ToTable("GroupMembers");
        builder.HasKey(member => new { member.GroupId, member.UserId });
        builder.Property(member => member.Role)
            .HasConversion<string>()
            .HasMaxLength(10)
            .IsRequired();
        builder.Property(member => member.JoinedAtUtc).IsRequired();
        builder.HasIndex(member => member.UserId);
        builder.HasIndex(member => member.GroupId)
            .IsUnique()
            .HasFilter("\"Role\" = 'Owner'")
            .HasDatabaseName("IX_GroupMembers_OneOwner");
    }
}
