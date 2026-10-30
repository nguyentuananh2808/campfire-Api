using Campfire.Modules.StrangerChat.Domain.Entities;
using Microsoft.EntityFrameworkCore;
namespace Campfire.Modules.StrangerChat.Infrastructure.Persistence;

public sealed class StrangerChatDbContext(DbContextOptions<StrangerChatDbContext> options) : DbContext(options)
{
    public const string Schema = "stranger_chat";
    public DbSet<GuestSession> Sessions => Set<GuestSession>(); public DbSet<MatchmakingTicket> Tickets => Set<MatchmakingTicket>(); public DbSet<StrangerConversation> Conversations => Set<StrangerConversation>(); public DbSet<StrangerMessage> Messages => Set<StrangerMessage>(); public DbSet<StrangerMessageReaction> Reactions => Set<StrangerMessageReaction>(); public DbSet<StrangerBlock> Blocks => Set<StrangerBlock>(); public DbSet<StrangerReport> Reports => Set<StrangerReport>();
    protected override void OnModelCreating(ModelBuilder b)
    {
        b.HasDefaultSchema(Schema);
        b.Entity<GuestSession>(e => { e.ToTable("guest_sessions", table => { table.HasCheckConstraint("CK_guest_sessions_Gender", "\"Gender\" IN ('Male', 'Female', 'Other')"); table.HasCheckConstraint("CK_guest_sessions_GenderPreference", "\"GenderPreference\" IN ('Any', 'Male', 'Female', 'Other')"); }); e.HasKey(x => x.Id); e.Property(x => x.TokenHash).HasMaxLength(64); e.Property(x => x.Nickname).HasMaxLength(24); e.Property(x => x.Gender).HasConversion<string>().HasMaxLength(16); e.Property(x => x.GenderPreference).HasConversion<string>().HasMaxLength(16); e.HasIndex(x => x.TokenHash).IsUnique(); e.HasIndex(x => new { x.Gender, x.GenderPreference }); });
        b.Entity<MatchmakingTicket>(e => { e.ToTable("matchmaking_tickets"); e.HasKey(x => x.Id); e.HasIndex(x => x.SessionId).IsUnique(); e.HasIndex(x => new { x.QueuedAtUtc, x.ExpiresAtUtc }); });
        b.Entity<StrangerConversation>(e => { e.ToTable("conversations"); e.HasKey(x => x.Id); e.Ignore(x => x.IsActive); e.HasIndex(x => new { x.FirstSessionId, x.EndedAtUtc }); e.HasIndex(x => new { x.SecondSessionId, x.EndedAtUtc }); e.Property(x => x.Version).IsConcurrencyToken(); });
        b.Entity<StrangerMessage>(e => { e.ToTable("messages", table => table.HasCheckConstraint("CK_messages_Kind", "\"Kind\" IN ('Text', 'Sticker')")); e.HasKey(x => x.Id); e.Property(x => x.Kind).HasConversion<string>().HasMaxLength(16).HasDefaultValue(StrangerMessageKind.Text); e.Property(x => x.Content).HasMaxLength(1000); e.Property(x => x.Version).IsConcurrencyToken(); e.HasOne<StrangerMessage>().WithMany().HasForeignKey(x => x.ReplyToMessageId).OnDelete(DeleteBehavior.Restrict); e.HasIndex(x => new { x.ConversationId, x.SenderSessionId, x.ClientMessageId }).IsUnique(); e.HasIndex(x => new { x.ConversationId, x.SentAtUtc, x.Id }); });
        b.Entity<StrangerMessageReaction>(e => { e.ToTable("message_reactions", table => table.HasCheckConstraint("CK_message_reactions_Reaction", "\"Reaction\" IN ('Like', 'Love', 'Laugh', 'Wow', 'Sad')")); e.HasKey(x => new { x.MessageId, x.SessionId }); e.Property(x => x.Reaction).HasMaxLength(16); e.HasOne<StrangerMessage>().WithMany().HasForeignKey(x => x.MessageId).OnDelete(DeleteBehavior.Cascade); e.HasIndex(x => new { x.MessageId, x.Reaction }); });
        b.Entity<StrangerBlock>(e => { e.ToTable("blocks"); e.HasKey(x => x.Id); e.HasIndex(x => new { x.BlockerSessionId, x.BlockedSessionId }).IsUnique(); e.HasIndex(x => x.ExpiresAtUtc); });
        b.Entity<StrangerReport>(e => { e.ToTable("reports"); e.HasKey(x => x.Id); e.Property(x => x.Reason).HasMaxLength(32); e.Property(x => x.Note).HasMaxLength(500); e.HasIndex(x => new { x.ConversationId, x.ReporterSessionId }).IsUnique().HasDatabaseName("UX_reports_conversation_reporter"); e.HasIndex(x => x.RetainUntilUtc); });
    }
}
