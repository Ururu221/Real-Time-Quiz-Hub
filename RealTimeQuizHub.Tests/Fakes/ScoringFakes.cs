using RealTimeQuizHub.Models;
using RealTimeQuizHub.Repository.Interfaces;

namespace RealTimeQuizHub.Tests.Fakes
{
    // ===== In-memory IScoreRepository =====
    public class FakeScoreRepository : IScoreRepository
    {
        public readonly List<UserScore> Scores = new();
        public readonly Dictionary<int, UserStats> Stats = new();
        private int _nextId = 1;

        public Task<UserScore> AddScoreAsync(UserScore score)
        {
            score.Id = _nextId++;
            Scores.Add(score);
            return Task.FromResult(score);
        }

        public Task<List<UserScore>> GetRoomScoresAsync(int roomId)
            => Task.FromResult(Scores.Where(s => s.RoomId == roomId).ToList());

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

    // ===== In-memory IRoomRepository (only GetByIdAsync is exercised by scoring) =====
    public class FakeRoomRepositoryForScoring : IRoomRepository
    {
        private readonly Dictionary<int, Room> _rooms = new();

        public void Add(Room room) => _rooms[room.Id] = room;

        public Task<Room?> GetByIdAsync(int id)
            => Task.FromResult(_rooms.TryGetValue(id, out var r) ? r : null);

        public Task<List<Room>> GetAllActiveAsync()
            => Task.FromResult(_rooms.Values.Where(r => r.IsActive).ToList());

        public Task<Room> AddAsync(Room room)
        {
            _rooms[room.Id] = room;
            return Task.FromResult(room);
        }

        public Task<bool> DeleteAsync(int id) => Task.FromResult(_rooms.Remove(id));
    }
}
