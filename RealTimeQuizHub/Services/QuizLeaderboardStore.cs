using System.Collections.Concurrent;
using RealTimeQuizHub.Services.Interfaces;

namespace RealTimeQuizHub.Services
{
    // Thread-safe in-memory store. Registered as a singleton so it survives between
    // requests, but it holds no persistence — restarting the app clears every
    // per-quiz leaderboard. Keyed strictly by quizId, so each quiz is independent.
    public class QuizLeaderboardStore : IQuizLeaderboardStore
    {
        // quizId -> (userId -> that user's best entry for the quiz)
        private readonly ConcurrentDictionary<int, ConcurrentDictionary<int, QuizScoreEntry>> _byQuiz = new();

        public int Record(int quizId, int userId, int score, DateTime completedAt)
        {
            var quizBoard = _byQuiz.GetOrAdd(quizId, _ => new ConcurrentDictionary<int, QuizScoreEntry>());

            var entry = new QuizScoreEntry(userId, score, completedAt);
            // Keep only the user's best score for this quiz.
            quizBoard.AddOrUpdate(userId, entry, (_, existing) =>
                score > existing.Score ? entry : existing);

            var myBest = quizBoard[userId].Score;
            // Rank = how many distinct users have a strictly higher best score, plus one.
            var ahead = quizBoard.Values.Count(e => e.UserId != userId && e.Score > myBest);
            return ahead + 1;
        }

        public IReadOnlyList<QuizScoreEntry> GetTop(int quizId, int top)
        {
            if (!_byQuiz.TryGetValue(quizId, out var quizBoard))
            {
                return Array.Empty<QuizScoreEntry>();
            }

            return quizBoard.Values
                .OrderByDescending(e => e.Score)
                .ThenBy(e => e.CompletedAt)
                .Take(top)
                .ToList();
        }
    }
}
