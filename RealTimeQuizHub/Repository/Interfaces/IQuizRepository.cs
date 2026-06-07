using RealTimeQuizHub.Models;

namespace RealTimeQuizHub.Repository.Interfaces
{
    public interface IQuizRepository
    {
        Task<List<Quiz>> GetAllAsync();
        Task<Quiz?> GetByIdAsync(int id);
        Task<Quiz> AddAsync(Quiz quiz);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
    }
}
