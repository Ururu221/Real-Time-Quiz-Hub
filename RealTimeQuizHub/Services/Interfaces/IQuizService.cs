using RealTimeQuizHub.Models;
using RealTimeQuizHub.Models.Dtos;

namespace RealTimeQuizHub.Services.Interfaces
{
    public interface IQuizService
    {
        Task<List<QuizListDto>> GetAllAsync();
        Task<QuizListDto?> GetByIdAsync(int id);
        Task<Quiz> CreateQuizAsync(CreateQuizDto dto, int createdByUserId);
        Task<bool> DeleteQuizAsync(int id);

        // Ordered questions (with answers) that belong to a specific quiz,
        // resolved through the quiz_questions join table.
        Task<List<Question>> GetQuizQuestionsAsync(int quizId);
    }
}
