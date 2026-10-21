namespace Campfire.Modules.Groups.Application.Common;

public static class GroupFailures
{
    public static GroupFailure HiddenGroup() => new(
        GroupFailureKind.NotFound,
        "group_not_found",
        "Không tìm thấy nhóm.");

    public static GroupFailure OwnerRequired() => new(
        GroupFailureKind.Forbidden,
        "owner_required",
        "Chỉ chủ nhóm mới có thể thực hiện thao tác này.");

    public static GroupFailure MemberNotFound() => new(
        GroupFailureKind.NotFound,
        "member_not_found",
        "Không tìm thấy thành viên trong nhóm.");

    public static GroupFailure OwnerMustTransfer() => new(
        GroupFailureKind.Conflict,
        "owner_must_transfer",
        "Chủ nhóm phải chuyển quyền cho thành viên khác trước khi rời nhóm.");

    public static GroupFailure OwnershipConflict() => new(
        GroupFailureKind.Conflict,
        "group_version_conflict",
        "Nhóm đã thay đổi. Hãy tải lại và thử chuyển quyền lần nữa.");

    public static GroupFailure InvitationUnavailable() => new(
        GroupFailureKind.NotFound,
        "invitation_unavailable",
        "Liên kết mời không tồn tại, đã hết hạn hoặc đã bị thu hồi.");
}
