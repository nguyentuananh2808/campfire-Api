using System.Buffers.Binary;

namespace Campfire.Modules.Chat.Infrastructure.Services;

internal static class ChatCursorCodec
{
    public static string Encode(DateTimeOffset sentAtUtc, Guid messageId)
    {
        Span<byte> payload = stackalloc byte[24];
        BinaryPrimitives.WriteInt64BigEndian(payload, sentAtUtc.UtcTicks);
        messageId.TryWriteBytes(payload[8..]);
        return Convert.ToBase64String(payload).TrimEnd('=').Replace('+', '-').Replace('/', '_');
    }

    public static bool TryDecode(string? value, out ChatCursor cursor)
    {
        cursor = default;
        if (string.IsNullOrWhiteSpace(value)) return false;
        try
        {
            var normalized = value.Replace('-', '+').Replace('_', '/');
            normalized += new string('=', (4 - normalized.Length % 4) % 4);
            var payload = Convert.FromBase64String(normalized);
            if (payload.Length != 24) return false;
            var ticks = BinaryPrimitives.ReadInt64BigEndian(payload.AsSpan(0, 8));
            cursor = new ChatCursor(new DateTimeOffset(ticks, TimeSpan.Zero), new Guid(payload.AsSpan(8, 16)));
            return true;
        }
        catch (FormatException) { return false; }
        catch (ArgumentOutOfRangeException) { return false; }
    }
}
