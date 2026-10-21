using Campfire.Modules.Identity.Application;
using Campfire.Modules.Identity.Application.Abstractions;
using Campfire.Modules.Identity.Application.Common;
using Campfire.Modules.Identity.Application.Register;
using Campfire.Modules.Identity.Contracts;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Campfire.UnitTests;

public sealed class IdentityMediatorTests
{
    [Fact]
    public async Task Sender_dispatches_register_command_to_handler_and_injected_port()
    {
        var fake = new FakeIdentityService();
        await using var provider = new ServiceCollection()
            .AddLogging()
            .AddIdentityApplication()
            .AddSingleton<IIdentityService>(fake)
            .BuildServiceProvider();

        var sender = provider.GetRequiredService<ISender>();
        var result = await sender.Send(new RegisterCommand(
            "friend@example.com",
            "StrongPassword!1",
            "Friend"));

        Assert.True(result.IsSuccess);
        Assert.True(fake.RegisterCalled);
        Assert.Equal("friend@example.com", result.Value?.Email);
    }

    [Fact]
    public async Task Validation_pipeline_stops_invalid_command_before_handler()
    {
        var fake = new FakeIdentityService();
        await using var provider = new ServiceCollection()
            .AddLogging()
            .AddIdentityApplication()
            .AddSingleton<IIdentityService>(fake)
            .BuildServiceProvider();

        var sender = provider.GetRequiredService<ISender>();
        var exception = await Assert.ThrowsAsync<RequestValidationException>(() =>
            sender.Send(new RegisterCommand("not-an-email", "short", "x")));

        Assert.False(fake.RegisterCalled);
        Assert.Contains("email", exception.Errors.Keys);
        Assert.Contains("password", exception.Errors.Keys);
        Assert.Contains("displayName", exception.Errors.Keys);
    }

    private sealed class FakeIdentityService : IIdentityService
    {
        public bool RegisterCalled { get; private set; }

        public Task<IdentityResult<UserAccount>> RegisterAsync(
            string email,
            string password,
            string displayName,
            CancellationToken cancellationToken)
        {
            RegisterCalled = true;
            return Task.FromResult(IdentityResult<UserAccount>.Success(
                new UserAccount(Guid.NewGuid(), email, displayName)));
        }

        public Task<IdentityResult<UserAccount>> LoginAsync(
            string email,
            string password,
            CancellationToken cancellationToken) => throw new InvalidOperationException("Not used by this test.");

        public Task<IdentityResult<UserAccount>> GetCurrentUserAsync(
            Guid userId,
            CancellationToken cancellationToken) => throw new InvalidOperationException("Not used by this test.");

        public Task<IdentityResult<bool>> ChangePasswordAsync(
            Guid userId,
            string currentPassword,
            string newPassword,
            CancellationToken cancellationToken) => throw new InvalidOperationException("Not used by this test.");

        public Task<IdentityResult<bool>> LogoutAsync(CancellationToken cancellationToken) =>
            throw new InvalidOperationException("Not used by this test.");
    }
}
