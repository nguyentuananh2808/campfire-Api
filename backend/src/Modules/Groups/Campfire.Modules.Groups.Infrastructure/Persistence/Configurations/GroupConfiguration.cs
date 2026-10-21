using Campfire.Modules.Groups.Domain.Groups;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Campfire.Modules.Groups.Infrastructure.Persistence.Configurations;

internal sealed class GroupConfiguration : IEntityTypeConfiguration<Group>
{
    public void Configure(EntityTypeBuilder<Group> builder)
    {
        builder.ToTable("Groups");
        builder.HasKey(group => group.Id);
        builder.Property(group => group.Name).HasMaxLength(80).IsRequired();
        builder.Property(group => group.CreatedAtUtc).IsRequired();
        builder.Property(group => group.Version).IsConcurrencyToken().IsRequired();
        builder.HasMany(group => group.Members)
            .WithOne()
            .HasForeignKey(member => member.GroupId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(group => group.Members).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
