using Microsoft.EntityFrameworkCore;
using RealTimeQuizHub.Models;
using RealTimeQuizHub.Repository.Interfaces;

namespace RealTimeQuizHub.Repository
{
    public class ScoreRepository : IScoreRepository
    {
        private readonly AppDbContext _db;
        public ScoreRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task<UserStats?> GetStatsAsync(int userId)
        {
            return await _db.UserStats.FirstOrDefaultAsync(s => s.UserId == userId);
        }

        public async Task AddStatsAsync(UserStats stats)
        {
            _db.UserStats.Add(stats);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateStatsAsync(UserStats stats)
        {
            _db.UserStats.Update(stats);
            await _db.SaveChangesAsync();
        }

        public async Task<List<UserStats>> GetTopStatsAsync(int count)
        {
            return await _db.UserStats
                .Include(s => s.User)
                .OrderByDescending(s => s.TotalScore)
                .ThenByDescending(s => s.Wins)
                .Take(count)
                .ToListAsync();
        }

        public async Task<List<UserStats>> GetStatsForUsersAsync(IEnumerable<int> userIds)
        {
            var ids = userIds.Distinct().ToList();
            return await _db.UserStats
                .Include(s => s.User)
                .Where(s => ids.Contains(s.UserId))
                .ToListAsync();
        }
    }
}
