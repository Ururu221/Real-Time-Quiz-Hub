using RealTimeQuizHub.Models;

namespace RealTimeQuizHub.Services.Interfaces
{
    public interface IQuestionService
    {
        Task<List<Question>> GetAllAsync();

        Task<Question> GetQuestionByIdAsync(int questionId);
    }
}
