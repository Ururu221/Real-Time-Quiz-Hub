using RealTimeQuizHub.Models;

namespace RealTimeQuizHub.Repository.Interfaces
{
    public interface IRoomRepository
    {
        // Active rooms with their quiz (and quiz questions) eagerly loaded.
        Task<List<Room>> GetAllActiveAsync();
        Task<Room?> GetByIdAsync(int id);
        Task<Room> AddAsync(Room room);
        Task<bool> DeleteAsync(int id);
    }
}
