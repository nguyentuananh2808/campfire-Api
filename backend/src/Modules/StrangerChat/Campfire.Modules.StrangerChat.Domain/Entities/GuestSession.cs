namespace Campfire.Modules.StrangerChat.Domain.Entities;

public sealed class GuestSession
{
    private GuestSession() { }
    private GuestSession(string tokenHash, string nickname, GuestGender gender, GenderPreference genderPreference, DateTimeOffset now)
    {
        Id = Guid.NewGuid(); TokenHash = tokenHash; Nickname = nickname.Trim(); Gender = gender; GenderPreference = genderPreference; CreatedAtUtc = now; LastSeenAtUtc = now;
    }
    public Guid Id { get; private set; }
    public string TokenHash { get; private set; } = string.Empty;
    public string Nickname { get; private set; } = string.Empty;
    public GuestGender Gender { get; private set; } = GuestGender.Other;
    public GenderPreference GenderPreference { get; private set; } = GenderPreference.Any;
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset LastSeenAtUtc { get; private set; }
    public static GuestSession Create(string tokenHash, string nickname, GuestGender gender, GenderPreference genderPreference, DateTimeOffset now)
    {
        if (string.IsNullOrWhiteSpace(nickname) || nickname.Trim().Length is < 2 or > 24) throw new ArgumentException("Nickname must be 2-24 characters.");
        return new GuestSession(tokenHash, nickname, gender, genderPreference, now);
    }
    public void Touch(DateTimeOffset now) => LastSeenAtUtc = now;
}
