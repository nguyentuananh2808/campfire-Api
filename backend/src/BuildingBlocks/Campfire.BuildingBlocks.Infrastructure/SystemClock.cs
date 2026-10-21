using Campfire.BuildingBlocks.Application;

namespace Campfire.BuildingBlocks.Infrastructure;

public sealed class SystemClock : IClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
