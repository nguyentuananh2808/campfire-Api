using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Campfire.Modules.Chat.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Campfire.IntegrationTests;

public sealed class ChatApiTests(CampfireApiFactory factory) : IClassFixture<CampfireApiFactory>
{
    [Fact]
    public async Task Inbox_persists_unread_counts_and_mark_read_is_member_scoped()
    {
        using var owner = factory.CreateClient();
        using var member = factory.CreateClient();
        using var outsider = factory.CreateClient();
        await RegisterAsync(owner, "Chủ nhóm");
        await RegisterAsync(member, "Thành viên");
        await RegisterAsync(outsider, "Người ngoài");

        var groupResponse = await owner.PostAsJsonAsync("/api/v1/groups", new { name = "Hội cuối tuần" });
        var groupId = GetGuid(await groupResponse.Content.ReadAsStringAsync(), "id");
        var invitation = await owner.PostAsJsonAsync(
            $"/api/v1/groups/{groupId}/invitations",
            new { expiresInHours = 1 });
        var token = GetString(await invitation.Content.ReadAsStringAsync(), "token");
        Assert.Equal(
            HttpStatusCode.OK,
            (await member.PostAsJsonAsync("/api/v1/group-invitations/join", new { token })).StatusCode);

        await owner.PostAsJsonAsync($"/api/v1/groups/{groupId}/messages", new
        {
            clientMessageId = Guid.NewGuid(),
            content = "Tin thứ nhất"
        });
        var latestResponse = await owner.PostAsJsonAsync($"/api/v1/groups/{groupId}/messages", new
        {
            clientMessageId = Guid.NewGuid(),
            content = "Tin mới nhất"
        });
        var latestMessageId = (await latestResponse.Content.ReadFromJsonAsync<JsonElement>())
            .GetProperty("id").GetGuid();

        var inbox = await member.GetFromJsonAsync<JsonElement[]>("/api/v1/chat/inbox");
        var groupInbox = Assert.Single(inbox!);
        Assert.Equal(groupId, groupInbox.GetProperty("groupId").GetGuid());
        Assert.Equal(2, groupInbox.GetProperty("unreadCount").GetInt32());
        Assert.Equal("Tin mới nhất", groupInbox.GetProperty("lastMessagePreview").GetString());

        var marked = await member.PutAsJsonAsync(
            $"/api/v1/groups/{groupId}/messages/read",
            new { messageId = latestMessageId });
        Assert.Equal(HttpStatusCode.OK, marked.StatusCode);
        inbox = await member.GetFromJsonAsync<JsonElement[]>("/api/v1/chat/inbox");
        Assert.Equal(0, Assert.Single(inbox!).GetProperty("unreadCount").GetInt32());

        Assert.Equal(
            HttpStatusCode.NotFound,
            (await outsider.PutAsJsonAsync(
                $"/api/v1/groups/{groupId}/messages/read",
                new { messageId = latestMessageId })).StatusCode);
    }

    [Fact]
    public async Task Send_is_member_scoped_idempotent_and_conflicts_for_changed_payload()
    {
        using var member = factory.CreateClient();
        using var outsider = factory.CreateClient();
        await RegisterAsync(member, "Người nhắn");
        await RegisterAsync(outsider, "Người ngoài");
        var groupResponse = await member.PostAsJsonAsync("/api/v1/groups", new { name = "Chat test" });
        var groupId = GetGuid(await groupResponse.Content.ReadAsStringAsync(), "id");
        var clientMessageId = Guid.NewGuid();

        var first = await member.PostAsJsonAsync($"/api/v1/groups/{groupId}/messages", new { clientMessageId, content = "Xin chào <b>nhóm</b>" });
        var retry = await member.PostAsJsonAsync($"/api/v1/groups/{groupId}/messages", new { clientMessageId, content = "Xin chào <b>nhóm</b>" });
        var conflict = await member.PostAsJsonAsync($"/api/v1/groups/{groupId}/messages", new { clientMessageId, content = "Nội dung khác" });
        var firstBody = await first.Content.ReadFromJsonAsync<JsonElement>();
        var firstMessageId = firstBody.GetProperty("id").GetGuid();
        var firstVersion = firstBody.GetProperty("version").GetGuid();
        var edited = await member.PutAsJsonAsync(
            $"/api/v1/groups/{groupId}/messages/{firstMessageId}",
            new { content = "Xin chào nhóm (đã sửa)", expectedVersion = firstVersion });
        var staleEdit = await member.PutAsJsonAsync(
            $"/api/v1/groups/{groupId}/messages/{firstMessageId}",
            new { content = "Bản sửa bị trễ", expectedVersion = firstVersion });
        var reply = await member.PostAsJsonAsync($"/api/v1/groups/{groupId}/messages", new
        {
            clientMessageId = Guid.NewGuid(),
            content = "Mình đồng ý",
            replyToMessageId = firstMessageId
        });
        var reaction = await member.PutAsJsonAsync(
            $"/api/v1/groups/{groupId}/messages/{firstMessageId}/reaction",
            new { reaction = "Love" });
        var outsiderReaction = await outsider.PutAsJsonAsync(
            $"/api/v1/groups/{groupId}/messages/{firstMessageId}/reaction",
            new { reaction = "Like" });
        var stickerMessageId = Guid.NewGuid();
        var sticker = await member.PostAsJsonAsync($"/api/v1/groups/{groupId}/messages", new
        {
            clientMessageId = stickerMessageId,
            kind = "Sticker",
            content = "campfire"
        });
        var stickerRetry = await member.PostAsJsonAsync($"/api/v1/groups/{groupId}/messages", new
        {
            clientMessageId = stickerMessageId,
            kind = "Sticker",
            content = "campfire"
        });
        var stickerBody = await sticker.Content.ReadFromJsonAsync<JsonElement>();
        var stickerEdit = await member.PutAsJsonAsync(
            $"/api/v1/groups/{groupId}/messages/{stickerBody.GetProperty("id").GetGuid()}",
            new { content = "Không được phép", expectedVersion = stickerBody.GetProperty("version").GetGuid() });
        var changedKind = await member.PostAsJsonAsync($"/api/v1/groups/{groupId}/messages", new
        {
            clientMessageId = stickerMessageId,
            kind = "Text",
            content = "campfire"
        });
        var unknownSticker = await member.PostAsJsonAsync($"/api/v1/groups/{groupId}/messages", new
        {
            clientMessageId = Guid.NewGuid(),
            kind = "Sticker",
            content = "unknown"
        });

        Assert.Equal(HttpStatusCode.Created, first.StatusCode);
        Assert.Equal(HttpStatusCode.OK, edited.StatusCode);
        Assert.NotNull((await edited.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("editedAtUtc").GetString());
        Assert.Equal(HttpStatusCode.Conflict, staleEdit.StatusCode);
        Assert.Equal(HttpStatusCode.OK, retry.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, conflict.StatusCode);
        Assert.Equal(HttpStatusCode.Created, reply.StatusCode);
        Assert.Equal(firstMessageId, (await reply.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("replyTo").GetProperty("messageId").GetGuid());
        Assert.Equal(HttpStatusCode.OK, reaction.StatusCode);
        Assert.Equal("Love", (await reaction.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("myReaction").GetString());
        Assert.Equal(HttpStatusCode.NotFound, outsiderReaction.StatusCode);
        Assert.Equal(HttpStatusCode.Created, sticker.StatusCode);
        Assert.Equal(HttpStatusCode.OK, stickerRetry.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, stickerEdit.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, changedKind.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, unknownSticker.StatusCode);
        Assert.Equal("Sticker", GetString(await sticker.Content.ReadAsStringAsync(), "kind"));
        Assert.Equal(HttpStatusCode.NotFound, (await outsider.GetAsync($"/api/v1/groups/{groupId}/messages")).StatusCode);
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ChatDbContext>();
        Assert.Equal(3, await dbContext.Messages.CountAsync(message => message.GroupId == groupId));
        Assert.Single(await dbContext.Reactions.Where(item => item.MessageId == firstMessageId).ToListAsync());
    }

    private static async Task RegisterAsync(HttpClient client, string displayName)
    {
        await PrimeAntiforgeryAsync(client);
        (await client.PostAsJsonAsync("/api/v1/auth/register", new
        {
            email = $"{Guid.NewGuid():N}@example.com",
            password = "StrongPassword!1",
            displayName
        })).EnsureSuccessStatusCode();
        await PrimeAntiforgeryAsync(client);
    }

    private static async Task PrimeAntiforgeryAsync(HttpClient client)
    {
        var response = await client.GetAsync("/api/v1/auth/antiforgery");
        response.EnsureSuccessStatusCode();
        var cookie = response.Headers.GetValues("Set-Cookie").Single(value => value.StartsWith("XSRF-TOKEN=", StringComparison.Ordinal));
        var token = Uri.UnescapeDataString(cookie.Split(';', 2)[0]["XSRF-TOKEN=".Length..]);
        client.DefaultRequestHeaders.Remove("X-XSRF-TOKEN");
        client.DefaultRequestHeaders.Add("X-XSRF-TOKEN", token);
    }

    private static Guid GetGuid(string json, string propertyName)
    {
        using var document = JsonDocument.Parse(json);
        return document.RootElement.GetProperty(propertyName).GetGuid();
    }

    private static string GetString(string json, string propertyName)
    {
        using var document = JsonDocument.Parse(json);
        return document.RootElement.GetProperty(propertyName).GetString()!;
    }
}
