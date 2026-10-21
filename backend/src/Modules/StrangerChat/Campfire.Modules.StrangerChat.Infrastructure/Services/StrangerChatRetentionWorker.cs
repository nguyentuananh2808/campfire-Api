using Campfire.Modules.StrangerChat.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Campfire.Modules.StrangerChat.Infrastructure.Services;

public sealed class StrangerChatRetentionWorker(
    IServiceScopeFactory scopeFactory,
    TimeProvider clock,
    ILogger<StrangerChatRetentionWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromHours(1), clock);
        do
        {
            try
            {
                await CleanupAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Stranger Chat retention cleanup failed");
            }
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    private async Task CleanupAsync(CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<StrangerChatDbContext>();
        var now = clock.GetUtcNow();
        var expiredSessionIds = db.Sessions
            .Where(session => session.LastSeenAtUtc <= now.AddHours(-24))
            .Select(session => session.Id);

        await db.Conversations
            .Where(conversation => conversation.EndedAtUtc == null &&
                (expiredSessionIds.Contains(conversation.FirstSessionId) || expiredSessionIds.Contains(conversation.SecondSessionId)))
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(conversation => conversation.EndedAtUtc, now)
                .SetProperty(conversation => conversation.Version, Guid.NewGuid()), cancellationToken);

        await db.Tickets.Where(ticket => ticket.ExpiresAtUtc <= now || expiredSessionIds.Contains(ticket.SessionId)).ExecuteDeleteAsync(cancellationToken);
        await db.Blocks.Where(block => block.ExpiresAtUtc <= now).ExecuteDeleteAsync(cancellationToken);
        await db.Reports.Where(report => report.RetainUntilUtc <= now).ExecuteDeleteAsync(cancellationToken);

        var disposableConversationIds = db.Conversations
            .Where(conversation => conversation.EndedAtUtc <= now.AddHours(-24) && !db.Reports.Any(report => report.ConversationId == conversation.Id))
            .Select(conversation => conversation.Id);
        await db.Messages.Where(message => disposableConversationIds.Contains(message.ConversationId)).ExecuteDeleteAsync(cancellationToken);
        await db.Conversations.Where(conversation => disposableConversationIds.Contains(conversation.Id)).ExecuteDeleteAsync(cancellationToken);
        await db.Sessions.Where(session => expiredSessionIds.Contains(session.Id)).ExecuteDeleteAsync(cancellationToken);
    }
}
