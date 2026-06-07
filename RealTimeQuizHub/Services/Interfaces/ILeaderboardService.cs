using RealTimeQuizHub.Models.Dtos;

namespace RealTimeQuizHub.Services.Interfaces
{
    public interface ILeaderboardService
    {
        // Top users overall, ordered by total score (with level and badges).
        Task<List<LeaderboardEntryDto>> GetGlobalLeaderboardAsync(int top = 20);

        // Participants of a single quiz, ranked by their best session score.
        Task<List<QuizLeaderboardEntryDto>> GetQuizLeaderboardAsync(int quizId, int top = 20);
    }
}
