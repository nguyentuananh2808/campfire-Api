using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Campfire.Modules.StrangerChat.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Campfire.IntegrationTests;

public sealed class StrangerChatApiTests(CampfireApiFactory factory) : IClassFixture<CampfireApiFactory>
{
    [Fact]
    public async Task Matchmaking_requires_mutually_compatible_gender_preferences()
    {
        using var womanLookingForMan = factory.CreateClient();
        using var womanLookingForWoman = factory.CreateClient();
        using var manLookingForWoman = factory.CreateClient();

        await CreateSessionAsync(womanLookingForMan, "Bạn nữ A", "Female", "Male");
        await CreateSessionAsync(womanLookingForWoman, "Bạn nữ B", "Female", "Female");
        await CreateSessionAsync(manLookingForWoman, "Bạn nam C", "Male", "Female");

        Assert.Equal("waiting", await QueueAndReadStatusAsync(womanLookingForMan));
        Assert.Equal("waiting", await QueueAndReadStatusAsync(womanLookingForWoman));
        Assert.Equal("matched", await QueueAndReadStatusAsync(manLookingForWoman));

        var firstConversation = await womanLookingForMan.GetAsync("/api/v1/stranger/conversation");
        Assert.Equal(HttpStatusCode.OK, firstConversation.StatusCode);
        Assert.Contains("Bạn nam C", await firstConversation.Content.ReadAsStringAsync(), StringComparison.Ordinal);

        Assert.Equal(HttpStatusCode.NotFound, (await womanLookingForWoman.GetAsync("/api/v1/stranger/conversation")).StatusCode);
    }

    [Fact]
    public async Task Session_rejects_unknown_gender_values()
    {
        using var client = factory.CreateClient();
        await PrimeAntiforgeryAsync(client);

        var response = await client.PostAsJsonAsync("/api/v1/stranger/session", new
        {
            nickname = "Người thử",
            gender = "Unknown",
            genderPreference = "Any",
            confirmsAdult = true
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Guest_mutation_requires_antiforgery_and_session_metadata_is_private()
    {
        using var client = factory.CreateClient();
        var rejected = await client.PostAsJsonAsync("/api/v1/stranger/session", new
        {
            nickname = "Người thử",
            gender = "Other",
            genderPreference = "Any",
            confirmsAdult = true
        });

        Assert.Equal(HttpStatusCode.BadRequest, rejected.StatusCode);
        Assert.Equal(
            "antiforgery_validation_failed",
            (await rejected.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("code").GetString());

        await PrimeAntiforgeryAsync(client);
        var created = await client.PostAsJsonAsync("/api/v1/stranger/session", new
        {
            nickname = "Người thử",
            gender = "Other",
            genderPreference = "Any",
            confirmsAdult = true
        });

        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        Assert.Equal("no-store", created.Headers.CacheControl?.ToString());
        Assert.Equal("DENY", created.Headers.GetValues("X-Frame-Options").Single());
        Assert.Equal("nosniff", created.Headers.GetValues("X-Content-Type-Options").Single());

        var metadata = await created.Content.ReadFromJsonAsync<JsonElement>();
        Assert.False(metadata.TryGetProperty("id", out _));
        Assert.False(metadata.TryGetProperty("rawToken", out _));
    }

    [Fact]
    public async Task Report_is_idempotent_and_ends_the_active_conversation()
    {
        using var first = factory.CreateClient();
        using var second = factory.CreateClient();
        await CreateSessionAsync(first, "Người A", "Other", "Any");
        await CreateSessionAsync(second, "Người B", "Other", "Any");

        Assert.Equal("waiting", await QueueAndReadStatusAsync(first));
        var match = await second.PostAsJsonAsync("/api/v1/stranger/matchmaking", new { });
        match.EnsureSuccessStatusCode();
        var matchBody = await match.Content.ReadFromJsonAsync<JsonElement>();
        var conversationId = matchBody.GetProperty("conversation").GetProperty("id").GetGuid();

        var firstReport = await second.PostAsJsonAsync(
            $"/api/v1/stranger/conversations/{conversationId}/reports",
            new { reason = "harassment", note = "Nội dung kiểm thử" });
        var retry = await second.PostAsJsonAsync(
            $"/api/v1/stranger/conversations/{conversationId}/reports",
            new { reason = "harassment", note = "Nội dung kiểm thử" });

        Assert.Equal(HttpStatusCode.OK, firstReport.StatusCode);
        Assert.Equal(HttpStatusCode.OK, retry.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await first.GetAsync("/api/v1/stranger/conversation")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await second.GetAsync("/api/v1/stranger/conversation")).StatusCode);

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<StrangerChatDbContext>();
        Assert.Equal(
            1,
            await db.Reports.CountAsync(report =>
                report.ConversationId == conversationId));
    }

    [Fact]
    public async Task Participants_can_send_sticker_reply_and_set_one_reaction()
    {
        using var first = factory.CreateClient();
        using var second = factory.CreateClient();
        await CreateSessionAsync(first, "Người A", "Other", "Any");
        await CreateSessionAsync(second, "Người B", "Other", "Any");
        Assert.Equal("waiting", await QueueAndReadStatusAsync(first));
        var match = await second.PostAsJsonAsync("/api/v1/stranger/matchmaking", new { });
        var conversationId = (await match.Content.ReadFromJsonAsync<JsonElement>())
            .GetProperty("conversation").GetProperty("id").GetGuid();

        var firstMessage = await first.PostAsJsonAsync(
            $"/api/v1/stranger/conversations/{conversationId}/messages",
            new { clientMessageId = Guid.NewGuid(), kind = "Sticker", content = "campfire" });
        firstMessage.EnsureSuccessStatusCode();
        var firstMessageId = (await firstMessage.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetGuid();
        var reply = await second.PostAsJsonAsync(
            $"/api/v1/stranger/conversations/{conversationId}/messages",
            new { clientMessageId = Guid.NewGuid(), kind = "Text", content = "Chào bạn", replyToMessageId = firstMessageId });
        var reaction = await second.PutAsJsonAsync(
            $"/api/v1/stranger/conversations/{conversationId}/messages/{firstMessageId}/reaction",
            new { reaction = "Like" });

        Assert.Equal(HttpStatusCode.OK, reply.StatusCode);
        Assert.Equal(firstMessageId, (await reply.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("replyTo").GetProperty("messageId").GetGuid());
        Assert.Equal(HttpStatusCode.OK, reaction.StatusCode);
        Assert.Equal("Like", (await reaction.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("myReaction").GetString());

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<StrangerChatDbContext>();
        Assert.Equal(2, await db.Messages.CountAsync(message => message.ConversationId == conversationId));
        Assert.Single(await db.Reactions.Where(item => item.MessageId == firstMessageId).ToListAsync());
    }

    [Fact]
    public async Task Sender_can_edit_text_with_version_but_partner_and_stale_writer_cannot()
    {
        using var first = factory.CreateClient();
        using var second = factory.CreateClient();
        await CreateSessionAsync(first, "Người A", "Other", "Any");
        await CreateSessionAsync(second, "Người B", "Other", "Any");
        Assert.Equal("waiting", await QueueAndReadStatusAsync(first));
        var match = await second.PostAsJsonAsync("/api/v1/stranger/matchmaking", new { });
        var conversationId = (await match.Content.ReadFromJsonAsync<JsonElement>())
            .GetProperty("conversation").GetProperty("id").GetGuid();
        var sent = await first.PostAsJsonAsync(
            $"/api/v1/stranger/conversations/{conversationId}/messages",
            new { clientMessageId = Guid.NewGuid(), kind = "Text", content = "Ban đầu" });
        var body = await sent.Content.ReadFromJsonAsync<JsonElement>();
        var messageId = body.GetProperty("id").GetGuid();
        var version = body.GetProperty("version").GetGuid();

        var partnerEdit = await second.PutAsJsonAsync(
            $"/api/v1/stranger/conversations/{conversationId}/messages/{messageId}",
            new { content = "Không có quyền", expectedVersion = version });
        var edited = await first.PutAsJsonAsync(
            $"/api/v1/stranger/conversations/{conversationId}/messages/{messageId}",
            new { content = "Đã sửa", expectedVersion = version });
        var stale = await first.PutAsJsonAsync(
            $"/api/v1/stranger/conversations/{conversationId}/messages/{messageId}",
            new { content = "Bản cũ", expectedVersion = version });

        Assert.Equal(HttpStatusCode.Forbidden, partnerEdit.StatusCode);
        Assert.Equal(HttpStatusCode.OK, edited.StatusCode);
        var editedBody = await edited.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("Đã sửa", editedBody.GetProperty("content").GetString());
        Assert.NotNull(editedBody.GetProperty("editedAtUtc").GetString());
        Assert.Equal(HttpStatusCode.Conflict, stale.StatusCode);
    }

    private static async Task CreateSessionAsync(HttpClient client, string nickname, string gender, string preference)
    {
        await PrimeAntiforgeryAsync(client);
        var response = await client.PostAsJsonAsync("/api/v1/stranger/session", new
        {
            nickname,
            gender,
            genderPreference = preference,
            confirmsAdult = true
        });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var metadata = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.False(metadata.TryGetProperty("id", out _));
        Assert.False(metadata.TryGetProperty("rawToken", out _));
    }

    private static async Task<string> QueueAndReadStatusAsync(HttpClient client)
    {
        var response = await client.PostAsJsonAsync("/api/v1/stranger/matchmaking", new { });
        response.EnsureSuccessStatusCode();
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return document.RootElement.GetProperty("status").GetString()!;
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
}
