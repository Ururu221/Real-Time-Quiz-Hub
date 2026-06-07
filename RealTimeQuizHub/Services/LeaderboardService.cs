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
        private readonly IQuizLeaderboardStore _quizLeaderboard;

        public LeaderboardService(IScoreRepository scoreRepository,
                                  IBadgeRepository badgeRepository,
                                  IQuizLeaderboardStore quizLeaderboard)
        {
            _scoreRepository = scoreRepository;
            _badgeRepository = badgeRepository;
            _quizLeaderboard = quizLeaderboard;
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

        public async Task<List<QuizLeaderboardEntryDto>> GetQuizLeaderboardAsync(int quizId, int top = 20)
        {
            // Per-quiz standings come from the in-memory store (RAM, per quiz).
            var entries = _quizLeaderboard.GetTop(quizId, top);
            if (entries.Count == 0)
            {
                return new List<QuizLeaderboardEntryDto>();
            }

            // Enrich each row with the player's global profile (name, level, badges)
            // which IS persisted in the database.
            var userIds = entries.Select(e => e.UserId).ToList();
            var statsByUser = (await _scoreRepository.GetStatsForUsersAsync(userIds))
                .ToDictionary(s => s.UserId);
            var badgesByUser = await _badgeRepository.GetBadgesForUsersAsync(userIds);

            return entries.Select(e =>
            {
                statsByUser.TryGetValue(e.UserId, out var stats);
                return new QuizLeaderboardEntryDto
                {
                    UserId = e.UserId,
                    Name = stats?.User?.Name ?? string.Empty,
                    Score = e.Score,
                    CompletedAt = e.CompletedAt,
                    TotalScore = stats?.TotalScore ?? 0,
                    Level = LevelHelper.GetLevel(stats?.TotalScore ?? 0),
                    QuizzesCompleted = stats?.QuizzesCompleted ?? 0,
                    Wins = stats?.Wins ?? 0,
                    Badges = ToBadgeDtos(badgesByUser, e.UserId)
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
