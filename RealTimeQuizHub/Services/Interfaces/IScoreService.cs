using RealTimeQuizHub.Models;
using RealTimeQuizHub.Models.Dtos;

namespace RealTimeQuizHub.Services.Interfaces
{
    public interface IScoreService
    {
        // Base points awarded for a single correct answer.
        const int BaseScore = 100;

        // Points for one answer, applying the timer bonus when the room uses a timer.
        int CalculateAnswerScore(bool hasTimer, double timerScoreImpact, int timeRemaining, int timerSecondsPerQuestion);

        // Total score for a whole session, given the room's timer configuration.
        int CalculateSessionScore(Room room, IEnumerable<AnswerResultDto> answers);

        // Records a finished quiz: stores the score, updates the user's stats,
        // works out the room rank, and triggers a badge check.
        Task<QuizCompletionResultDto> RecordCompletionAsync(int userId, CompleteQuizDto dto);
    }
}
