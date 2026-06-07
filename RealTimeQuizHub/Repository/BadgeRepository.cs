using Microsoft.EntityFrameworkCore;
using RealTimeQuizHub.Models;
using RealTimeQuizHub.Repository.Interfaces;

namespace RealTimeQuizHub.Repository
{
    public class BadgeRepository : IBadgeRepository
    {
        private readonly AppDbContext _db;
        public BadgeRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task<List<Badge>> GetAllAsync()
        {
            return await _db.Badges.OrderBy(b => b.Id).ToListAsync();
        }

        public async Task<List<Badge>> GetUserBadgesAsync(int userId)
        {
            return await _db.UserBadges
                .Where(ub => ub.UserId == userId)
                .Include(ub => ub.Badge)
                .Select(ub => ub.Badge!)
                .ToListAsync();
        }

        public async Task AddUserBadgeAsync(UserBadge userBadge)
        {
            _db.UserBadges.Add(userBadge);
            await _db.SaveChangesAsync();
        }

        public async Task<Dictionary<int, List<Badge>>> GetBadgesForUsersAsync(IEnumerable<int> userIds)
        {
            var ids = userIds.Distinct().ToList();
            var rows = await _db.UserBadges
                .Where(ub => ids.Contains(ub.UserId))
                .Include(ub => ub.Badge)
                .ToListAsync();

            return rows
                .GroupBy(ub => ub.UserId)
                .ToDictionary(g => g.Key, g => g.Select(ub => ub.Badge!).ToList());
        }
    }
}
