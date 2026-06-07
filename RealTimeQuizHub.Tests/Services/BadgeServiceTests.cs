using RealTimeQuizHub.Models;
using RealTimeQuizHub.Services;
using RealTimeQuizHub.Tests.Fakes;

namespace RealTimeQuizHub.Tests.Services
{
    public class BadgeServiceTests
    {
        private static BadgeService CreateService(
            out FakeBadgeRepository badgeRepo,
            out FakeScoreRepository scoreRepo)
        {
            badgeRepo = new FakeBadgeRepository();
            scoreRepo = new FakeScoreRepository();
            return new BadgeService(badgeRepo, scoreRepo);
        }

        [Fact]
        public async Task FirstWin_IsAwarded_AfterFirstWin()
        {
            var svc = CreateService(out var badgeRepo, out var scoreRepo);
            // The stats already reflect the win that just happened.
            scoreRepo.Stats[1] = new UserStats { UserId = 1, TotalScore = 100, QuizzesCompleted = 1, Wins = 1 };

            var awarded = await svc.CheckAndAwardBadgesAsync(new BadgeCheckContext
            {
                UserId = 1,
                WonThisGame = true,
                TimerRoom = false,
                PerfectScore = false
            });

            Assert.Contains(awarded, b => b.Name == BadgeCatalog.FirstWin);
            var awardedFirstWinId = BadgeCatalog.Seed.First(s => s.Name == BadgeCatalog.FirstWin).Id;
            Assert.Single(badgeRepo.UserBadges, ub => ub.UserId == 1 && ub.BadgeId == awardedFirstWinId);
        }

        [Fact]
        public async Task Badge_IsNotDuplicated_IfAlreadyEarned()
        {
            var svc = CreateService(out var badgeRepo, out var scoreRepo);
            scoreRepo.Stats[1] = new UserStats { UserId = 1, TotalScore = 100, QuizzesCompleted = 1, Wins = 1 };

            // First check awards "Перша перемога".
            var first = await svc.CheckAndAwardBadgesAsync(new BadgeCheckContext { UserId = 1, WonThisGame = true });
            Assert.Contains(first, b => b.Name == BadgeCatalog.FirstWin);

            // Second check on the same state must not award it again.
            var second = await svc.CheckAndAwardBadgesAsync(new BadgeCheckContext { UserId = 1, WonThisGame = true });

            Assert.DoesNotContain(second, b => b.Name == BadgeCatalog.FirstWin);
            var firstWinId = BadgeCatalog.Seed.First(s => s.Name == BadgeCatalog.FirstWin).Id;
            Assert.Single(badgeRepo.UserBadges, ub => ub.UserId == 1 && ub.BadgeId == firstWinId);
        }

        [Fact]
        public async Task Perfectionist_IsAwarded_OnPerfectScore()
        {
            var svc = CreateService(out _, out var scoreRepo);
            scoreRepo.Stats[1] = new UserStats { UserId = 1, QuizzesCompleted = 1, Wins = 0 };

            var awarded = await svc.CheckAndAwardBadgesAsync(new BadgeCheckContext
            {
                UserId = 1,
                WonThisGame = false,
                PerfectScore = true
            });

            Assert.Contains(awarded, b => b.Name == BadgeCatalog.Perfectionist);
            Assert.DoesNotContain(awarded, b => b.Name == BadgeCatalog.FirstWin);
        }
    }
}
