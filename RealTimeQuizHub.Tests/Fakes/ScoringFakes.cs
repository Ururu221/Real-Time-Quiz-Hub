using RealTimeQuizHub.Models;
using RealTimeQuizHub.Repository.Interfaces;

namespace RealTimeQuizHub.Tests.Fakes
{
    // ===== In-memory IScoreRepository (global stats only) =====
    public class FakeScoreRepository : IScoreRepository
    {
        public readonly Dictionary<int, UserStats> Stats = new();

        public Task<UserStats?> GetStatsAsync(int userId)
            => Task.FromResult(Stats.TryGetValue(userId, out var s) ? s : null);

        public Task AddStatsAsync(UserStats stats)
        {
            Stats[stats.UserId] = stats;
            return Task.CompletedTask;
        }

        public Task UpdateStatsAsync(UserStats stats)
        {
            Stats[stats.UserId] = stats;
            return Task.CompletedTask;
        }

        public Task<List<UserStats>> GetTopStatsAsync(int count)
            => Task.FromResult(Stats.Values
                .OrderByDescending(s => s.TotalScore)
                .ThenByDescending(s => s.Wins)
                .Take(count)
                .ToList());

        public Task<List<UserStats>> GetStatsForUsersAsync(IEnumerable<int> userIds)
        {
            var ids = userIds.ToHashSet();
            return Task.FromResult(Stats.Values.Where(s => ids.Contains(s.UserId)).ToList());
        }
    }

    // ===== In-memory IBadgeRepository (seeded from the real catalogue) =====
    public class FakeBadgeRepository : IBadgeRepository
    {
        public readonly List<Badge> Badges =
            BadgeCatalog.Seed.Select(b => new Badge
            {
                Id = b.Id, Name = b.Name, Description = b.Description, IconEmoji = b.IconEmoji
            }).ToList();

        public readonly List<UserBadge> UserBadges = new();

        public Task<List<Badge>> GetAllAsync() => Task.FromResult(Badges.ToList());

        public Task<List<Badge>> GetUserBadgesAsync(int userId)
        {
            var badges = UserBadges
                .Where(ub => ub.UserId == userId)
                .Select(ub => Badges.First(b => b.Id == ub.BadgeId))
                .ToList();
            return Task.FromResult(badges);
        }

        public Task AddUserBadgeAsync(UserBadge userBadge)
        {
            userBadge.Badge = Badges.First(b => b.Id == userBadge.BadgeId);
            UserBadges.Add(userBadge);
            return Task.CompletedTask;
        }

        public Task<Dictionary<int, List<Badge>>> GetBadgesForUsersAsync(IEnumerable<int> userIds)
        {
            var ids = userIds.ToHashSet();
            var dict = UserBadges
                .Where(ub => ids.Contains(ub.UserId))
                .GroupBy(ub => ub.UserId)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(ub => Badges.First(b => b.Id == ub.BadgeId)).ToList());
            return Task.FromResult(dict);
        }
    }

    // ===== In-memory IQuizRepository (only GetByIdAsync is exercised by scoring) =====
    public class FakeQuizRepository : IQuizRepository
    {
        private readonly Dictionary<int, Quiz> _quizzes = new();

        public void Add(Quiz quiz) => _quizzes[quiz.Id] = quiz;

        public Task<Quiz?> GetByIdAsync(int id)
            => Task.FromResult(_quizzes.TryGetValue(id, out var q) ? q : null);

        public Task<List<Quiz>> GetAllAsync()
            => Task.FromResult(_quizzes.Values.ToList());

        public Task<Quiz> AddAsync(Quiz quiz)
        {
            _quizzes[quiz.Id] = quiz;
            return Task.FromResult(quiz);
        }

        public Task<bool> ExistsAsync(int id) => Task.FromResult(_quizzes.ContainsKey(id));

        public Task<bool> DeleteAsync(int id) => Task.FromResult(_quizzes.Remove(id));
    }
}
