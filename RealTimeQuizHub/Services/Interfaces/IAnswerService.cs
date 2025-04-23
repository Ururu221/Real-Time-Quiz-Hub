using RealTimeQuizHub.Models;

namespace RealTimeQuizHub.Services.Interfaces
{
    public interface IAnswerService
    {
        Task<List<Answer>> GetAllAnswersAsync();
        Task<Answer> GetAnswerByIdAsync(int answerId);
        Task<bool> AddAnswerAsync(Answer answer);
        Task<bool> UpdateAnswerAsync(Answer answer);
        Task<bool> DeleteAnswerAsync(int answerId);
        Task<List<Answer>> GetAnswersByQuestionIdAsync(int questionId);
    }
}
