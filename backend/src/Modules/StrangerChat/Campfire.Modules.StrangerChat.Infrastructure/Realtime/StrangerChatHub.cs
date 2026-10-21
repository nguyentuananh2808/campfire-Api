using Campfire.Modules.StrangerChat.Application.UseCases;
using MediatR;
using Microsoft.AspNetCore.SignalR;

namespace Campfire.Modules.StrangerChat.Infrastructure.Realtime;

public sealed class StrangerChatHub(ISender sender) : Hub
{
    public override async Task OnConnectedAsync()
    {
        var token = Context.GetHttpContext()?.Request.Cookies["Campfire.Guest"];
        if (string.IsNullOrWhiteSpace(token))
        {
            Context.Abort();
            return;
        }

        var conversationId = await sender.Send(
            new ResolveActiveConversationQuery(token),
            Context.ConnectionAborted);
        if (conversationId is null)
        {
            Context.Abort();
            return;
        }

        await Groups.AddToGroupAsync(
            Context.ConnectionId,
            Group(conversationId.Value),
            Context.ConnectionAborted);
        await base.OnConnectedAsync();
    }

    public static string Group(Guid id) => $"stranger:{id:N}";
}
