namespace Campfire.Modules.Werewolf.Application.Common;

public static class WerewolfFailures
{
    public static WerewolfFailure Hidden() => new("werewolf_room_not_found", "Không tìm thấy phòng hoặc bạn không có quyền truy cập.", 404);
    public static WerewolfFailure Forbidden() => new("werewolf_forbidden", "Bạn không có quyền thực hiện thao tác này.", 403);
    public static WerewolfFailure Conflict(string detail = "Trạng thái phòng đã thay đổi. Hãy tải lại.") => new("werewolf_conflict", detail, 409);
    public static WerewolfFailure Full() => new("werewolf_room_full", "Phòng đã đủ người chơi.", 409);
    public static WerewolfFailure InvalidPhase() => new("werewolf_invalid_phase", "Hành động không hợp lệ trong giai đoạn hiện tại.", 409);
    public static WerewolfFailure VoiceUnavailable() => new("werewolf_voice_unavailable", "Voice server chưa được cấu hình hoặc bạn không có kênh voice trong giai đoạn này.", 503);
}
