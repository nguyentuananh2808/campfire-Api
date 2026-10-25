using Campfire.Modules.Chat.Domain.Messages;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Campfire.Modules.Chat.Infrastructure.Persistence.Configurations;

public sealed class ChatMessageReactionConfiguration : IEntityTypeConfiguration<ChatMessageReaction>
{
    public void Configure(EntityTypeBuilder<ChatMessageReaction> builder)
    {
        builder.ToTable("MessageReactions", ChatDbContext.Schema, table =>
            table.HasCheckConstraint("CK_MessageReactions_Reaction", "\"Reaction\" IN ('Like', 'Love', 'Laugh', 'Wow', 'Sad')"));
        builder.HasKey(reaction => new { reaction.MessageId, reaction.UserId });
        builder.Property(reaction => reaction.Reaction).HasMaxLength(16).IsRequired();
        builder.Property(reaction => reaction.ReactedAtUtc).HasColumnType("timestamp with time zone");
        builder.HasOne<ChatMessage>().WithMany().HasForeignKey(reaction => reaction.MessageId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(reaction => new { reaction.MessageId, reaction.Reaction });
    }
}
