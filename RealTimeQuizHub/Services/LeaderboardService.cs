using RealTimeQuizHub.Models;
using RealTimeQuizHub.Models.Dtos;
using RealTimeQuizHub.Repository.Interfaces;
using RealTimeQuizHub.Services.Interfaces;

namespace RealTimeQuizHub.Services
{
    public class LeaderboardService : ILeaderboardService
    {
        private readonly IScoreRepository _scoreRepository;
        private readonly IBadgeRepository _badgeRepository;

        public LeaderboardService(IScoreRepository scoreRepository, IBadgeRepository badgeRepository)
        {
            _scoreRepository = scoreRepository;
            _badgeRepository = badgeRepository;
        }

        public async Task<List<LeaderboardEntryDto>> GetGlobalLeaderboardAsync(int top = 20)
        {
            var stats = await _scoreRepository.GetTopStatsAsync(top);
            if (stats.Count == 0)
            {
                return new List<LeaderboardEntryDto>();
            }

            var badgesByUser = await _badgeRepository.GetBadgesForUsersAsync(stats.Select(s => s.UserId));

            return stats.Select(s => new LeaderboardEntryDto
            {
                UserId = s.UserId,
                Name = s.User?.Name ?? string.Empty,
                TotalScore = s.TotalScore,
                Level = LevelHelper.GetLevel(s.TotalScore),
                QuizzesCompleted = s.QuizzesCompleted,
                Wins = s.Wins,
                Badges = ToBadgeDtos(badgesByUser, s.UserId)
            }).ToList();
        }

        public async Task<List<RoomLeaderboardEntryDto>> GetRoomLeaderboardAsync(int roomId)
        {
            var scores = await _scoreRepository.GetRoomScoresAsync(roomId);
            if (scores.Count == 0)
            {
                return new List<RoomLeaderboardEntryDto>();
            }

            // Each participant keeps their best score in this room.
            var bestPerUser = scores
                .GroupBy(s => s.UserId)
                .Select(g => new { UserId = g.Key, Score = g.Max(s => s.Score) })
                .OrderByDescending(x => x.Score)
                .ToList();

            var userIds = bestPerUser.Select(x => x.UserId).ToList();
            var statsByUser = (await _scoreRepository.GetStatsForUsersAsync(userIds))
                .ToDictionary(s => s.UserId);
            var badgesByUser = await _badgeRepository.GetBadgesForUsersAsync(userIds);

            return bestPerUser.Select(x =>
            {
                statsByUser.TryGetValue(x.UserId, out var stats);
                return new RoomLeaderboardEntryDto
                {
                    UserId = x.UserId,
                    Name = stats?.User?.Name ?? string.Empty,
                    Score = x.Score,
                    TotalScore = stats?.TotalScore ?? 0,
                    Level = LevelHelper.GetLevel(stats?.TotalScore ?? 0),
                    QuizzesCompleted = stats?.QuizzesCompleted ?? 0,
                    Wins = stats?.Wins ?? 0,
                    Badges = ToBadgeDtos(badgesByUser, x.UserId)
                };
            }).ToList();
        }

        private static List<BadgeDto> ToBadgeDtos(Dictionary<int, List<Badge>> badgesByUser, int userId)
        {
            if (!badgesByUser.TryGetValue(userId, out var badges))
            {
                return new List<BadgeDto>();
            }
            return badges.Select(b => new BadgeDto
            {
                Name = b.Name,
                Description = b.Description,
                IconEmoji = b.IconEmoji
            }).ToList();
        }
    }
}
