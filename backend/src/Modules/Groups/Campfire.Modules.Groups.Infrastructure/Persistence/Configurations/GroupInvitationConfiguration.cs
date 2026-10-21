using Campfire.Modules.Groups.Domain.Groups;
using Campfire.Modules.Groups.Domain.Invitations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Campfire.Modules.Groups.Infrastructure.Persistence.Configurations;

internal sealed class GroupInvitationConfiguration : IEntityTypeConfiguration<GroupInvitation>
{
    public void Configure(EntityTypeBuilder<GroupInvitation> builder)
    {
        builder.ToTable("GroupInvitations");
        builder.HasKey(invitation => invitation.Id);
        builder.Property(invitation => invitation.TokenHash).HasMaxLength(32).IsRequired();
        builder.Property(invitation => invitation.CreatedAtUtc).IsRequired();
        builder.Property(invitation => invitation.ExpiresAtUtc).IsRequired();
        builder.HasIndex(invitation => invitation.TokenHash).IsUnique();
        builder.HasIndex(invitation => new { invitation.GroupId, invitation.ExpiresAtUtc });
        builder.HasOne<Group>()
            .WithMany()
            .HasForeignKey(invitation => invitation.GroupId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
