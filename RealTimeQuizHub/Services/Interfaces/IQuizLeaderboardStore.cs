namespace RealTimeQuizHub.Services.Interfaces
{
    // A single recorded result in a quiz's in-memory leaderboard.
    public record QuizScoreEntry(int UserId, int Score, DateTime CompletedAt);

    // Per-quiz leaderboards live ONLY in memory (RAM): they are isolated per quiz
    // and reset whenever the app restarts. The global leaderboard, by contrast,
    // is persisted in PostgreSQL via user_stats.
    public interface IQuizLeaderboardStore
    {
        // Records a finished session for a quiz, keeping each user's best score.
        // Returns the user's current rank in that quiz (1 = first place).
        int Record(int quizId, int userId, int score, DateTime completedAt);

        // Top entries for a single quiz, best score first.
        IReadOnlyList<QuizScoreEntry> GetTop(int quizId, int top);
    }
}
