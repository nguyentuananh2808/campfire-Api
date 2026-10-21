using Campfire.Modules.Chat.Infrastructure.Persistence;
using Campfire.Modules.Events.Infrastructure.Persistence;
using Campfire.Modules.Groups.Infrastructure.Persistence;
using Campfire.Modules.Identity.Infrastructure.Persistence;
using Campfire.Modules.Werewolf.Infrastructure.Persistence;
using Campfire.Modules.StrangerChat.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

using var cancellationSource = new CancellationTokenSource();
Console.CancelKeyPress += (_, eventArgs) =>
{
    eventArgs.Cancel = true;
    cancellationSource.Cancel();
};

try
{
    await using var identityDbContext = new IdentityDbContextFactory().CreateDbContext(args);
    await identityDbContext.Database.MigrateAsync(cancellationSource.Token);
    Console.WriteLine("Identity migrations applied successfully.");
    await using var groupsDbContext = new GroupsDbContextFactory().CreateDbContext(args);
    await groupsDbContext.Database.MigrateAsync(cancellationSource.Token);
    Console.WriteLine("Groups migrations applied successfully.");
    await using var chatDbContext = new ChatDbContextFactory().CreateDbContext(args);
    await chatDbContext.Database.MigrateAsync(cancellationSource.Token);
    Console.WriteLine("Chat migrations applied successfully.");
    await using var eventsDbContext = new EventsDbContextFactory().CreateDbContext(args);
    await eventsDbContext.Database.MigrateAsync(cancellationSource.Token);
    Console.WriteLine("Events migrations applied successfully.");
    await using var werewolfDbContext = new WerewolfDbContextFactory().CreateDbContext(args);
    await werewolfDbContext.Database.MigrateAsync(cancellationSource.Token);
    Console.WriteLine("Werewolf migrations applied successfully.");
    await using var strangerChatDbContext = new StrangerChatDbContextFactory().CreateDbContext(args);
    await strangerChatDbContext.Database.MigrateAsync(cancellationSource.Token);
    Console.WriteLine("Stranger Chat migrations applied successfully.");
    return 0;
}
catch (OperationCanceledException)
{
    Console.Error.WriteLine("Migration was cancelled.");
    return 130;
}
catch (Exception exception)
{
    Console.Error.WriteLine($"Migration failed: {exception.GetType().Name}: {exception.Message}");
    return 1;
}
