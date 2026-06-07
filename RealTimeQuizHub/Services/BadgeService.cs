using RealTimeQuizHub.Models;
using RealTimeQuizHub.Repository.Interfaces;
using RealTimeQuizHub.Services.Interfaces;

namespace RealTimeQuizHub.Services
{
    public class BadgeService : IBadgeService
    {
        private readonly IBadgeRepository _badgeRepository;
        private readonly IScoreRepository _scoreRepository;

        public BadgeService(IBadgeRepository badgeRepository, IScoreRepository scoreRepository)
        {
            _badgeRepository = badgeRepository;
            _scoreRepository = scoreRepository;
        }

        public async Task<List<Badge>> CheckAndAwardBadgesAsync(BadgeCheckContext context)
        {
            // Cumulative achievements are decided from the user's running totals.
            var stats = await _scoreRepository.GetStatsAsync(context.UserId);
            var wins = stats?.Wins ?? 0;
            var quizzesCompleted = stats?.QuizzesCompleted ?? 0;

            // Work out which badge names the user qualifies for right now.
            var qualifies = new HashSet<string>();
            if (wins >= 1) qualifies.Add(BadgeCatalog.FirstWin);
            if (wins >= BadgeCatalog.UnbeatableWins) qualifies.Add(BadgeCatalog.Unbeatable);
            if (context.WonThisGame && context.TimerRoom) qualifies.Add(BadgeCatalog.Lightning);
            if (context.PerfectScore) qualifies.Add(BadgeCatalog.Perfectionist);
            if (quizzesCompleted >= BadgeCatalog.EnthusiastQuizzes) qualifies.Add(BadgeCatalog.Enthusiast);

            if (qualifies.Count == 0)
            {
                return new List<Badge>();
            }

            // Only award badges the user doesn't already hold (no duplicates).
            var alreadyEarned = (await _badgeRepository.GetUserBadgesAsync(context.UserId))
                .Select(b => b.Name)
                .ToHashSet();

            var catalogue = await _badgeRepository.GetAllAsync();

            var awarded = new List<Badge>();
            foreach (var badge in catalogue)
            {
                if (qualifies.Contains(badge.Name) && !alreadyEarned.Contains(badge.Name))
                {
                    await _badgeRepository.AddUserBadgeAsync(new UserBadge
                    {
                        UserId = context.UserId,
                        BadgeId = badge.Id,
                        EarnedAt = DateTime.UtcNow
                    });
                    awarded.Add(badge);
                }
            }

            return awarded;
        }
    }
}
