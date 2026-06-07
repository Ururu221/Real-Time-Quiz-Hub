using Microsoft.AspNetCore.Mvc;
using RealTimeQuizHub.Controllers;
using RealTimeQuizHub.Models;
using RealTimeQuizHub.Models.Dtos;
using RealTimeQuizHub.Services;
using RealTimeQuizHub.Tests.Fakes;

namespace RealTimeQuizHub.Tests.Controllers
{
    public class LeaderboardControllerTests
    {
        [Fact]
        public async Task GetGlobal_ReturnsRankedEntries_WithLevelAndBadges()
        {
            var scoreRepo = new FakeScoreRepository();
            var badgeRepo = new FakeBadgeRepository();

            // Two users: Bob (1500 → Досвідчений) should outrank Alice (300 → Новачок).
            scoreRepo.Stats[1] = new UserStats
            {
                UserId = 1, TotalScore = 300, QuizzesCompleted = 2, Wins = 0,
                User = new User { Id = 1, Name = "Alice" }
            };
            scoreRepo.Stats[2] = new UserStats
            {
                UserId = 2, TotalScore = 1500, QuizzesCompleted = 6, Wins = 3,
                User = new User { Id = 2, Name = "Bob" }
            };

            var firstWinId = BadgeCatalog.Seed.First(b => b.Name == BadgeCatalog.FirstWin).Id;
            badgeRepo.UserBadges.Add(new UserBadge { UserId = 2, BadgeId = firstWinId });

            var service = new LeaderboardService(scoreRepo, badgeRepo);
            var controller = new LeaderboardController(service);

            var result = await controller.GetGlobal();

            var ok = Assert.IsType<OkObjectResult>(result);
            var entries = Assert.IsAssignableFrom<List<LeaderboardEntryDto>>(ok.Value);

            Assert.Equal(2, entries.Count);

            // Highest total score first.
            var top = entries[0];
            Assert.Equal("Bob", top.Name);
            Assert.Equal(1500, top.TotalScore);
            Assert.Equal("Досвідчений", top.Level);
            Assert.Equal(6, top.QuizzesCompleted);
            Assert.Equal(3, top.Wins);
            Assert.Single(top.Badges);
            Assert.Equal(BadgeCatalog.FirstWin, top.Badges[0].Name);
            Assert.Equal("🏆", top.Badges[0].IconEmoji);

            var second = entries[1];
            Assert.Equal("Alice", second.Name);
            Assert.Equal("Новачок", second.Level);
            Assert.Empty(second.Badges);
        }

        [Fact]
        public async Task GetGlobal_ReturnsEmptyList_WhenNoStats()
        {
            var service = new LeaderboardService(new FakeScoreRepository(), new FakeBadgeRepository());
            var controller = new LeaderboardController(service);

            var result = await controller.GetGlobal();

            var ok = Assert.IsType<OkObjectResult>(result);
            var entries = Assert.IsAssignableFrom<List<LeaderboardEntryDto>>(ok.Value);
            Assert.Empty(entries);
        }
    }
}
