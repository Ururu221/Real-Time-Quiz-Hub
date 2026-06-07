using RealTimeQuizHub.Models;
using RealTimeQuizHub.Models.Dtos;

namespace RealTimeQuizHub.Services.Interfaces
{
    public interface IScoreService
    {
        // Base points awarded for a single correct answer.
        const int BaseScore = 100;

        // Points for one answer, applying the timer bonus when the quiz uses a timer.
        int CalculateAnswerScore(bool hasTimer, double timerScoreImpact, int timeRemaining, int timerSecondsPerQuestion);

        // Total score for a whole session, given the quiz's timer configuration.
        int CalculateSessionScore(Quiz quiz, IEnumerable<AnswerResultDto> answers);

        // Records a finished quiz: stores the score, updates the user's stats,
        // works out the quiz rank, and triggers a badge check.
        Task<QuizCompletionResultDto> RecordCompletionAsync(int userId, CompleteQuizDto dto);
    }
}
