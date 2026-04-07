using RealTimeQuizHub.Models;

namespace RealTimeQuizHub.Services.Interfaces
{
    public interface IQuizSessionService
    {
        Task<QuizSession> StartQuizAsync(string quizId, string nickname);
        Task<Question> GetNextQuestionAsync(string quizId, string nickname);
        Task<bool> SubmitAnswerAsync(string quizId, string answer, string nickname);
        Task<QuizSession?> GetQuizSessionAsync(string quizId, string nickname);
        Task EndQuizAsync(string quizId, string nickname);
    }
}
