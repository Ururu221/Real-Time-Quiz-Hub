using RealTimeQuizHub.Models;

namespace RealTimeQuizHub.Repository.Interfaces
{
    // Persists the GLOBAL leaderboard data (user_stats) in PostgreSQL. Per-quiz
    // standings are NOT stored here — they live in IQuizLeaderboardStore (memory).
    public interface IScoreRepository
    {
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
