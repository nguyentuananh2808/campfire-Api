using Campfire.Modules.Chat.Domain.ReadStates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Campfire.Modules.Chat.Infrastructure.Persistence.Configurations;

public sealed class ChatReadStateConfiguration : IEntityTypeConfiguration<ChatReadState>
{
    public void Configure(EntityTypeBuilder<ChatReadState> builder)
    {
        builder.ToTable("ReadStates", ChatDbContext.Schema);
        builder.HasKey(state => new { state.GroupId, state.UserId });
        builder.Property(state => state.LastReadAtUtc).HasColumnType("timestamp with time zone");
        builder.HasIndex(state => state.UserId);
    }
}
