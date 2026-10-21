namespace Campfire.Modules.Events.Application.Common;

public static class EventFailures
{
    public static EventFailure HiddenResource() => new("event_not_found", "Không tìm thấy kèo hoặc bạn không còn là thành viên.", 404);
    public static EventFailure ManageForbidden() => new("event_manage_forbidden", "Chỉ người tạo kèo hoặc chủ nhóm mới có thể thực hiện thao tác này.", 403);
    public static EventFailure Conflict() => new("event_concurrency_conflict", "Kèo đã thay đổi. Hãy tải lại trước khi tiếp tục.", 409);
    public static EventFailure InvalidTransition() => new("event_transition_invalid", "Không thể chuyển sang trạng thái đã chọn.", 409);
    public static EventFailure RsvpLocked() => new("event_rsvp_locked", "Kèo đã chốt hoặc hủy nên không còn nhận RSVP.", 409);
    public static EventFailure FutureTimeRequired() => new("event_future_time_required", "Thời gian diễn ra phải ở tương lai.", 400);
    public static EventFailure EventLocked() => new("event_locked", "Kèo đã hủy nên không thể chỉnh sửa.", 409);
}
