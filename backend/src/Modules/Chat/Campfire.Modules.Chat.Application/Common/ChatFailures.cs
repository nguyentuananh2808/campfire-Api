namespace Campfire.Modules.Chat.Application.Common;

public static class ChatFailures
{
    public static ChatFailure HiddenGroup() => new("chat_group_not_found", "Không tìm thấy nhóm hoặc bạn không còn là thành viên.", 404);
    public static ChatFailure IdempotencyConflict() => new("chat_idempotency_conflict", "Mã gửi lại đã được dùng cho nội dung khác.", 409);
    public static ChatFailure InvalidCursor() => new("chat_cursor_invalid", "Cursor tin nhắn không hợp lệ.", 400);
}
