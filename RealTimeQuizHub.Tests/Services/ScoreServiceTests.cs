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
            out FakeQuizRepository quizRepo,
            out QuizLeaderboardStore quizLeaderboard)
        {
            scoreRepo = new FakeScoreRepository();
            badgeRepo = new FakeBadgeRepository();
            quizRepo = new FakeQuizRepository();
            quizLeaderboard = new QuizLeaderboardStore();
            var badgeService = new BadgeService(badgeRepo, scoreRepo);
            return new ScoreService(scoreRepo, quizRepo, badgeService, quizLeaderboard);
        }

        [Fact]
        public void CalculateAnswerScore_WithTimer_AppliesBonusFormula()
        {
            var svc = CreateService(out _, out _, out _, out _);

            // base=100, impact=0.5, 20s left of 30s → bonus = 100*0.5*(20/30) = 33 → 133
            var score = svc.CalculateAnswerScore(
                hasTimer: true, timerScoreImpact: 0.5, timeRemaining: 20, timerSecondsPerQuestion: 30);

            Assert.Equal(133, score);
        }

        [Fact]
        public void CalculateAnswerScore_WithoutTimer_IsFlatBase()
        {
            var svc = CreateService(out _, out _, out _, out _);

            var noTimer = svc.CalculateAnswerScore(false, 0.0, 0, 30);
            var zeroImpact = svc.CalculateAnswerScore(true, 0.0, 30, 30);

            Assert.Equal(100, noTimer);
            Assert.Equal(100, zeroImpact);
        }

        [Fact]
        public void CalculateSessionScore_SumsOnlyCorrectAnswers()
        {
            var svc = CreateService(out _, out _, out _, out _);
            var quiz = new Quiz { Id = 1, HasTimer = true, TimerScoreImpact = 0.5, TimerSecondsPerQuestion = 30 };

            var answers = new List<AnswerResultDto>
            {
                new() { IsCorrect = true,  TimeRemaining = 30 }, // 100 + 50 = 150
                new() { IsCorrect = false, TimeRemaining = 30 }, // ignored
                new() { IsCorrect = true,  TimeRemaining = 0  }  // 100 + 0  = 100
            };

            Assert.Equal(250, svc.CalculateSessionScore(quiz, answers));
        }

        [Fact]
        public async Task RecordCompletionAsync_PersistsScoreAndStats()
        {
            var svc = CreateService(out var scoreRepo, out _, out var quizRepo, out var quizLeaderboard);
            quizRepo.Add(new Quiz { Id = 7, HasTimer = false });

            var dto = new CompleteQuizDto
            {
                QuizId = 7,
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
            Assert.Equal(200, scoreRepo.Stats[42].TotalScore);

            // The per-quiz result is recorded in the in-memory store, not the DB.
            var quizTop = quizLeaderboard.GetTop(7, 20);
            Assert.Single(quizTop);
            Assert.Equal(42, quizTop[0].UserId);
            Assert.Equal(200, quizTop[0].Score);
        }

        [Fact]
        public async Task RecordCompletionAsync_KeepsQuizLeaderboardsSeparate()
        {
            var svc = CreateService(out _, out _, out var quizRepo, out var quizLeaderboard);
            quizRepo.Add(new Quiz { Id = 1, HasTimer = false });
            quizRepo.Add(new Quiz { Id = 2, HasTimer = false });

            // User 100 plays quiz 1; user 200 plays quiz 2.
            await svc.RecordCompletionAsync(100, new CompleteQuizDto
            {
                QuizId = 1,
                Answers = new List<AnswerResultDto> { new() { IsCorrect = true } }
            });
            await svc.RecordCompletionAsync(200, new CompleteQuizDto
            {
                QuizId = 2,
                Answers = new List<AnswerResultDto> { new() { IsCorrect = true }, new() { IsCorrect = true } }
            });

            var quiz1 = quizLeaderboard.GetTop(1, 20);
            var quiz2 = quizLeaderboard.GetTop(2, 20);

            // Each quiz has its own, different leaderboard.
            Assert.Single(quiz1);
            Assert.Equal(100, quiz1[0].UserId);
            Assert.Single(quiz2);
            Assert.Equal(200, quiz2[0].UserId);
            Assert.DoesNotContain(quiz2, e => e.UserId == 100);
        }
    }
}
