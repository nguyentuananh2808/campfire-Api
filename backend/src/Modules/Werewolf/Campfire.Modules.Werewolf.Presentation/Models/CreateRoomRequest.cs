namespace Campfire.Modules.Werewolf.Presentation.Models;

public sealed record CreateRoomRequest(string Name, string Visibility, int MinimumPlayers, int MaximumPlayers, bool DeadVoiceEnabled);
