using RealTimeQuizHub.Models.Dtos;

namespace RealTimeQuizHub.Services.Interfaces
{
    public interface ILeaderboardService
    {
        // Top users overall, ordered by total score (with level and badges).
        Task<List<LeaderboardEntryDto>> GetGlobalLeaderboardAsync(int top = 20);

        // Participants of a single room, ranked by their best session score.
        Task<List<RoomLeaderboardEntryDto>> GetRoomLeaderboardAsync(int roomId);
    }
}
