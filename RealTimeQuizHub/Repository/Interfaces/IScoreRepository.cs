using RealTimeQuizHub.Models;

namespace RealTimeQuizHub.Repository.Interfaces
{
    public interface IScoreRepository
    {
        // Persists a finished session's score row.
        Task<UserScore> AddScoreAsync(UserScore score);

        // All score rows for a room (used to compute per-room ranking).
        Task<List<UserScore>> GetRoomScoresAsync(int roomId);

        // Aggregated stats for a single user (null if they have none yet).
        Task<UserStats?> GetStatsAsync(int userId);

        // Inserts a brand-new stats row.
        Task AddStatsAsync(UserStats stats);

        // Persists changes to an already-tracked stats row.
        Task UpdateStatsAsync(UserStats stats);

        // Top users by total score, with their User navigation loaded.
        Task<List<UserStats>> GetTopStatsAsync(int count);

        // Stats for a specific set of users (used to enrich a room leaderboard).
        Task<List<UserStats>> GetStatsForUsersAsync(IEnumerable<int> userIds);
    }
}
