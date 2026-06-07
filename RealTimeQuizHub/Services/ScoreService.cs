using RealTimeQuizHub.Models;
using RealTimeQuizHub.Models.Dtos;
using RealTimeQuizHub.Repository.Interfaces;
using RealTimeQuizHub.Services.Interfaces;

namespace RealTimeQuizHub.Services
{
    public class ScoreService : IScoreService
    {
        private readonly IScoreRepository _scoreRepository;
        private readonly IQuizRepository _quizRepository;
        private readonly IBadgeService _badgeService;
        private readonly IQuizLeaderboardStore _quizLeaderboard;

        public ScoreService(IScoreRepository scoreRepository,
                            IQuizRepository quizRepository,
                            IBadgeService badgeService,
                            IQuizLeaderboardStore quizLeaderboard)
        {
            _scoreRepository = scoreRepository;
            _quizRepository = quizRepository;
            _badgeService = badgeService;
            _quizLeaderboard = quizLeaderboard;
        }

        // bonus = base * impact * (timeRemaining / secondsPerQuestion)
        // final = base + bonus. With no timer (or impact 0) this is a flat base score.
        public int CalculateAnswerScore(bool hasTimer, double timerScoreImpact, int timeRemaining, int timerSecondsPerQuestion)
        {
            var baseScore = IScoreService.BaseScore;

            if (!hasTimer || timerScoreImpact <= 0.0 || timerSecondsPerQuestion <= 0)
            {
                return baseScore;
            }

            // Clamp remaining time into [0, secondsPerQuestion] so a stray client
            // value can never push the bonus above its intended ceiling.
            var clamped = Math.Clamp(timeRemaining, 0, timerSecondsPerQuestion);
            var bonus = baseScore * timerScoreImpact * ((double)clamped / timerSecondsPerQuestion);
            return baseScore + (int)Math.Round(bonus, MidpointRounding.AwayFromZero);
        }

        public int CalculateSessionScore(Quiz quiz, IEnumerable<AnswerResultDto> answers)
        {
            var total = 0;
            foreach (var answer in answers)
            {
                if (!answer.IsCorrect)
                {
                    continue;
                }
                total += CalculateAnswerScore(
                    quiz.HasTimer,
                    quiz.TimerScoreImpact,
                    answer.TimeRemaining,
                    quiz.TimerSecondsPerQuestion);
            }
            return total;
        }

        public async Task<QuizCompletionResultDto> RecordCompletionAsync(int userId, CompleteQuizDto dto)
        {
            if (dto == null)
            {
                throw new ArgumentNullException(nameof(dto));
            }

            var quiz = await _quizRepository.GetByIdAsync(dto.QuizId);
            if (quiz == null)
            {
                throw new ArgumentException($"Quiz with ID {dto.QuizId} does not exist.", nameof(dto));
            }

            var answers = dto.Answers ?? new List<AnswerResultDto>();
            var score = CalculateSessionScore(quiz, answers);

            // Per-quiz leaderboard lives in memory only — record there and let it
            // return this user's rank within THIS quiz.
            var rank = _quizLeaderboard.Record(quiz.Id, userId, score, DateTime.UtcNow);
            var isWin = rank == 1;
            var perfectScore = answers.Count > 0 && answers.All(a => a.IsCorrect);

            var stats = await UpdateStatsAsync(userId, score, isWin);

            var newBadges = await _badgeService.CheckAndAwardBadgesAsync(new BadgeCheckContext
            {
                UserId = userId,
                WonThisGame = isWin,
                TimerRoom = quiz.HasTimer,
                PerfectScore = perfectScore
            });

            return new QuizCompletionResultDto
            {
                Score = score,
                Rank = rank,
                IsWin = isWin,
                TotalScore = stats.TotalScore,
                Level = stats.Level,
                QuizzesCompleted = stats.QuizzesCompleted,
                Wins = stats.Wins,
                NewBadges = newBadges.Select(b => new BadgeDto
                {
                    Name = b.Name,
                    Description = b.Description,
                    IconEmoji = b.IconEmoji
                }).ToList()
            };
        }

        private async Task<UserStats> UpdateStatsAsync(int userId, int score, bool isWin)
        {
            var stats = await _scoreRepository.GetStatsAsync(userId);
            if (stats == null)
            {
                stats = new UserStats
                {
                    UserId = userId,
                    TotalScore = score,
                    QuizzesCompleted = 1,
                    Wins = isWin ? 1 : 0
                };
                await _scoreRepository.AddStatsAsync(stats);
            }
            else
            {
                stats.TotalScore += score;
                stats.QuizzesCompleted += 1;
                if (isWin)
                {
                    stats.Wins += 1;
                }
                await _scoreRepository.UpdateStatsAsync(stats);
            }
            return stats;
        }
    }
}
