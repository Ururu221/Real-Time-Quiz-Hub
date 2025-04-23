using RealTimeQuizHub.Models;

namespace RealTimeQuizHub.Repository.Interfaces
{
    public interface IAnswerRepository
    {
        Task<List<Answer>> GetAllAsync();
        Task<Answer> GetAnswerByIdAsync(int answerId);
        Task<bool> AddAnswerAsync(Answer answer);
        Task<bool> UpdateAnswerAsync(Answer answer);
        Task<bool> DeleteAnswerAsync(int answerId);
    }
}
