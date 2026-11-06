using Campfire.Modules.Chat.Domain.Messages;
using Campfire.Modules.Chat.Domain.ReadStates;
using Microsoft.EntityFrameworkCore;

namespace Campfire.Modules.Chat.Infrastructure.Persistence;

public sealed class ChatDbContext(DbContextOptions<ChatDbContext> options) : DbContext(options)
{
    public const string Schema = "chat";
    public DbSet<ChatMessage> Messages => Set<ChatMessage>();
    public DbSet<ChatMessageReaction> Reactions => Set<ChatMessageReaction>();
    public DbSet<ChatReadState> ReadStates => Set<ChatReadState>();

    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ChatDbContext).Assembly);
}
