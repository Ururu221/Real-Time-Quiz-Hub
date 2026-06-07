using RealTimeQuizHub.Models;
using RealTimeQuizHub.Models.Dtos;
using RealTimeQuizHub.Repository.Interfaces;
using RealTimeQuizHub.Services.Interfaces;

namespace RealTimeQuizHub.Services
{
    public class ScoreService : IScoreService
    {
        private readonly IScoreRepository _scoreRepository;
        private readonly IRoomRepository _roomRepository;
        private readonly IBadgeService _badgeService;

        public ScoreService(IScoreRepository scoreRepository,
                            IRoomRepository roomRepository,
                            IBadgeService badgeService)
        {
            _scoreRepository = scoreRepository;
            _roomRepository = roomRepository;
            _badgeService = badgeService;
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

        public int CalculateSessionScore(Room room, IEnumerable<AnswerResultDto> answers)
        {
            var total = 0;
            foreach (var answer in answers)
            {
                if (!answer.IsCorrect)
                {
                    continue;
                }
                total += CalculateAnswerScore(
                    room.HasTimer,
                    room.TimerScoreImpact,
                    answer.TimeRemaining,
                    room.TimerSecondsPerQuestion);
            }
            return total;
        }

        public async Task<QuizCompletionResultDto> RecordCompletionAsync(int userId, CompleteQuizDto dto)
        {
            if (dto == null)
            {
                throw new ArgumentNullException(nameof(dto));
            }

            var room = await _roomRepository.GetByIdAsync(dto.RoomId);
            if (room == null)
            {
                throw new ArgumentException($"Room with ID {dto.RoomId} does not exist.", nameof(dto));
            }

            var answers = dto.Answers ?? new List<AnswerResultDto>();
            var score = CalculateSessionScore(room, answers);

            // Persist this session's score first so the ranking query sees it.
            await _scoreRepository.AddScoreAsync(new UserScore
            {
                UserId = userId,
                RoomId = room.Id,
                Score = score,
                CompletedAt = DateTime.UtcNow
            });

            var rank = await ComputeRankAsync(room.Id, userId);
            var isWin = rank == 1;
            var perfectScore = answers.Count > 0 && answers.All(a => a.IsCorrect);

            var stats = await UpdateStatsAsync(userId, score, isWin);

            var newBadges = await _badgeService.CheckAndAwardBadgesAsync(new BadgeCheckContext
            {
                UserId = userId,
                WonThisGame = isWin,
                TimerRoom = room.HasTimer,
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

        // Rank a user by their best score among the distinct players in the room.
        private async Task<int> ComputeRankAsync(int roomId, int userId)
        {
            var scores = await _scoreRepository.GetRoomScoresAsync(roomId);
            var bestPerUser = scores
                .GroupBy(s => s.UserId)
                .ToDictionary(g => g.Key, g => g.Max(s => s.Score));

            if (!bestPerUser.TryGetValue(userId, out var myBest))
            {
                return 1;
            }

            // Rank = how many distinct users scored strictly higher, plus one.
            var ahead = bestPerUser.Count(kv => kv.Key != userId && kv.Value > myBest);
            return ahead + 1;
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
