namespace Campfire.BuildingBlocks.Application;

public interface IClock
{
    DateTimeOffset UtcNow { get; }
}
