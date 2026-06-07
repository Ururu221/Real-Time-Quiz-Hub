using RealTimeQuizHub.Models;
using RealTimeQuizHub.Models.Dtos;
using RealTimeQuizHub.Services;
using RealTimeQuizHub.Tests.Fakes;

namespace RealTimeQuizHub.Tests.Services
{
    public class ScoreServiceTests
    {
        private static ScoreService CreateService(
            out FakeScoreRepository scoreRepo,
            out FakeBadgeRepository badgeRepo,
            out FakeRoomRepositoryForScoring roomRepo)
        {
            scoreRepo = new FakeScoreRepository();
            badgeRepo = new FakeBadgeRepository();
            roomRepo = new FakeRoomRepositoryForScoring();
            var badgeService = new BadgeService(badgeRepo, scoreRepo);
            return new ScoreService(scoreRepo, roomRepo, badgeService);
        }

        [Fact]
        public void CalculateAnswerScore_WithTimer_AppliesBonusFormula()
        {
            var svc = CreateService(out _, out _, out _);

            // base=100, impact=0.5, 20s left of 30s → bonus = 100*0.5*(20/30) = 33 → 133
            var score = svc.CalculateAnswerScore(
                hasTimer: true, timerScoreImpact: 0.5, timeRemaining: 20, timerSecondsPerQuestion: 30);

            Assert.Equal(133, score);
        }

        [Fact]
        public void CalculateAnswerScore_WithoutTimer_IsFlatBase()
        {
            var svc = CreateService(out _, out _, out _);

            var noTimer = svc.CalculateAnswerScore(false, 0.0, 0, 30);
            var zeroImpact = svc.CalculateAnswerScore(true, 0.0, 30, 30);

            Assert.Equal(100, noTimer);
            Assert.Equal(100, zeroImpact);
        }

        [Fact]
        public void CalculateSessionScore_SumsOnlyCorrectAnswers()
        {
            var svc = CreateService(out _, out _, out _);
            var room = new Room { Id = 1, HasTimer = true, TimerScoreImpact = 0.5, TimerSecondsPerQuestion = 30 };

            var answers = new List<AnswerResultDto>
            {
                new() { IsCorrect = true,  TimeRemaining = 30 }, // 100 + 50 = 150
                new() { IsCorrect = false, TimeRemaining = 30 }, // ignored
                new() { IsCorrect = true,  TimeRemaining = 0  }  // 100 + 0  = 100
            };

            Assert.Equal(250, svc.CalculateSessionScore(room, answers));
        }

        [Fact]
        public async Task RecordCompletionAsync_PersistsScoreAndStats()
        {
            var svc = CreateService(out var scoreRepo, out _, out var roomRepo);
            roomRepo.Add(new Room { Id = 7, HasTimer = false });

            var dto = new CompleteQuizDto
            {
                RoomId = 7,
                Answers = new List<AnswerResultDto>
                {
                    new() { IsCorrect = true },
                    new() { IsCorrect = true },
                    new() { IsCorrect = false }
                }
            };

            var result = await svc.RecordCompletionAsync(userId: 42, dto);

            Assert.Equal(200, result.Score);          // two correct, flat 100 each
            Assert.Equal(1, result.Rank);             // only player → rank 1
            Assert.True(result.IsWin);
            Assert.Equal(200, result.TotalScore);
            Assert.Equal(1, result.QuizzesCompleted);
            Assert.Equal("Новачок", result.Level);
            Assert.Single(scoreRepo.Scores);
            Assert.Equal(200, scoreRepo.Stats[42].TotalScore);
        }
    }
}
