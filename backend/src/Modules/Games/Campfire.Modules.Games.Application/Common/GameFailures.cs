namespace Campfire.Modules.Games.Application.Common;

public static class GameFailures
{
    public static GameFailure Hidden() => new("game_not_found", "Không tìm thấy ván chơi.", 404);
    public static GameFailure InvalidOpponent() => new("game_opponent_invalid", "Đối thủ phải là một thành viên khác trong nhóm.", 400);
    public static GameFailure Conflict() => new("game_concurrency_conflict", "Ván chơi vừa thay đổi. Hãy tải lại.", 409);
    public static GameFailure NotYourTurn() => new("game_not_your_turn", "Chưa tới lượt của bạn.", 409);
    public static GameFailure CellOccupied() => new("game_cell_occupied", "Ô này đã được đánh.", 409);
    public static GameFailure Finished() => new("game_finished", "Ván chơi đã kết thúc.", 409);
}
