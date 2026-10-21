using System.Security.Claims;
using Campfire.Modules.Groups.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Campfire.Modules.Chat.Infrastructure.Realtime;

[Authorize]
public sealed class ChatHub(IGroupMembershipService membershipService, ChatConnectionRegistry registry) : Hub
{
    public async Task Subscribe(Guid groupId)
    {
        if (!TryGetUserId(out var userId) || !(await membershipService.GetAccessAsync(groupId, userId, Context.ConnectionAborted)).IsMember)
            throw new HubException("Không thể tham gia kênh trò chuyện này.");
        await Groups.AddToGroupAsync(Context.ConnectionId, GroupName(groupId), Context.ConnectionAborted);
        registry.Add(Context.ConnectionId, userId, groupId);
    }

    public async Task Unsubscribe(Guid groupId)
    {
        if (!TryGetUserId(out var userId)) return;
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, GroupName(groupId), Context.ConnectionAborted);
        registry.Remove(Context.ConnectionId, userId, groupId);
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        registry.RemoveConnection(Context.ConnectionId);
        return base.OnDisconnectedAsync(exception);
    }

    internal static string GroupName(Guid groupId) => $"chat:{groupId:N}";
    private bool TryGetUserId(out Guid userId) => Guid.TryParse(Context.User?.FindFirstValue(ClaimTypes.NameIdentifier), out userId);
}
