namespace Campfire.Modules.Chat.Application.Common;

public static class ChatFailures
{
    public static ChatFailure HiddenGroup() => new("chat_group_not_found", "Không tìm thấy nhóm hoặc bạn không còn là thành viên.", 404);
    public static ChatFailure IdempotencyConflict() => new("chat_idempotency_conflict", "Mã gửi lại đã được dùng cho nội dung khác.", 409);
    public static ChatFailure InvalidCursor() => new("chat_cursor_invalid", "Cursor tin nhắn không hợp lệ.", 400);
    public static ChatFailure MessageNotFound() => new("chat_message_not_found", "Không tìm thấy tin nhắn trong nhóm này.", 404);
    public static ChatFailure MessageEditForbidden() => new("chat_message_edit_forbidden", "Bạn chỉ có thể sửa tin nhắn văn bản do chính mình gửi.", 403);
    public static ChatFailure MessageEditConflict() => new("chat_message_edit_conflict", "Tin nhắn đã được thay đổi. Vui lòng tải lại rồi thử lại.", 409);
}
