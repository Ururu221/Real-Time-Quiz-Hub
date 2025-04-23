using RealTimeQuizHub.Models;

namespace RealTimeQuizHub.Services.Interfaces
{
    public interface IJsonQuestionService
    {
        Task<List<Question>> GetAllAsync();

        Task<Question> GetQuestionByIdAsync(int questionId);
    }
}
