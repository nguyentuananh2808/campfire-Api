using Campfire.Modules.Chat.Domain.Messages;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Campfire.Modules.Chat.Infrastructure.Persistence.Configurations;

public sealed class ChatMessageConfiguration : IEntityTypeConfiguration<ChatMessage>
{
    public void Configure(EntityTypeBuilder<ChatMessage> builder)
    {
        builder.ToTable("Messages", ChatDbContext.Schema, table =>
            table.HasCheckConstraint("CK_Messages_Kind", "\"Kind\" IN ('Text', 'Sticker')"));
        builder.HasKey(message => message.Id);
        builder.Property(message => message.Kind).HasConversion<string>().HasMaxLength(16)
            .HasDefaultValue(ChatMessageKind.Text).IsRequired();
        builder.Property(message => message.Content).HasMaxLength(2000).IsRequired();
        builder.Property(message => message.SentAtUtc).HasColumnType("timestamp with time zone");
        builder.HasIndex(message => new { message.GroupId, message.SenderUserId, message.ClientMessageId }).IsUnique();
        builder.HasIndex(message => new { message.GroupId, message.SentAtUtc, message.Id });
    }
}
