using RealTimeQuizHub.Models;

namespace RealTimeQuizHub.Services.Interfaces
{
    public interface IQuizSessionService
    {
        Task<QuizSession> StartQuizAsync(string quizId);
        Task<Question> GetNextQuestionAsync(string quizId);
        Task<bool> SubmitAnswerAsync(string quizId, string answer);
        Task<QuizSession> GetQuizSessionAsync(string quizId);
        Task EndQuizAsync(string quizId);
    }
}
