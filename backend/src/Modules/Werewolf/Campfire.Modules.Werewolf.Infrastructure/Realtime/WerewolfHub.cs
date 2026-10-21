using System.Security.Claims;
using Campfire.Modules.Werewolf.Application.SetConnection;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Campfire.Modules.Werewolf.Infrastructure.Realtime;

[Authorize]
public sealed class WerewolfHub(ISender sender, WerewolfConnectionRegistry registry) : Hub
{
    public async Task Subscribe(Guid roomId)
    {
        if (!Guid.TryParse(Context.User?.FindFirstValue(ClaimTypes.NameIdentifier), out var userId)) throw new HubException("Phiên đăng nhập không hợp lệ.");

        if (Context.Items.TryGetValue("werewolf-room", out var currentValue) &&
            currentValue is Guid currentRoomId && currentRoomId != roomId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, Group(currentRoomId), Context.ConnectionAborted);
            if (registry.Remove(currentRoomId, userId, Context.ConnectionId))
                await sender.Send(new SetConnectionCommand(currentRoomId, userId, false), Context.ConnectionAborted);
            Context.Items.Remove("werewolf-room");
        }

        var firstConnection = registry.Add(roomId, userId, Context.ConnectionId);
        if (firstConnection)
        {
            var result = await sender.Send(new SetConnectionCommand(roomId, userId, true), Context.ConnectionAborted);
            if (!result.IsSuccess) { registry.Remove(roomId, userId, Context.ConnectionId); throw new HubException("Không thể tham gia cập nhật của phòng."); }
        }
        await Groups.AddToGroupAsync(Context.ConnectionId, Group(roomId), Context.ConnectionAborted);
        Context.Items["werewolf-room"] = roomId;
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (Context.Items.TryGetValue("werewolf-room", out var value) && value is Guid roomId &&
            Guid.TryParse(Context.User?.FindFirstValue(ClaimTypes.NameIdentifier), out var userId) &&
            registry.Remove(roomId, userId, Context.ConnectionId))
            await sender.Send(new SetConnectionCommand(roomId, userId, false), CancellationToken.None);
        await base.OnDisconnectedAsync(exception);
    }

    public static string Group(Guid roomId) => $"werewolf:{roomId:N}";
}
