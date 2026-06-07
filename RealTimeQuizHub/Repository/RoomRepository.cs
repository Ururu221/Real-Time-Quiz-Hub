using Microsoft.EntityFrameworkCore;
using RealTimeQuizHub.Models;
using RealTimeQuizHub.Repository.Interfaces;

namespace RealTimeQuizHub.Repository
{
    public class RoomRepository : IRoomRepository
    {
        private readonly AppDbContext _db;
        public RoomRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task<List<Room>> GetAllActiveAsync()
        {
            // AsSplitQuery loads the QuizQuestions collection in a separate SQL
            // query, so the row count can never be distorted by JOIN fan-out.
            // QuestionCount is then an exact count of quiz_questions for the quiz.
            return await _db.Rooms
                .Where(r => r.IsActive)
                .Include(r => r.Quiz!)
                    .ThenInclude(q => q.QuizQuestions)
                .OrderByDescending(r => r.CreatedAt)
                .AsSplitQuery()
                .ToListAsync();
        }

        public async Task<Room?> GetByIdAsync(int id)
        {
            return await _db.Rooms
                .Include(r => r.Quiz!)
                    .ThenInclude(q => q.QuizQuestions)
                        .ThenInclude(qq => qq.Question)
                .AsSplitQuery()
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<Room> AddAsync(Room room)
        {
            _db.Rooms.Add(room);
            await _db.SaveChangesAsync();
            return room;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var room = await _db.Rooms.FindAsync(id);
            if (room == null)
            {
                return false;
            }
            _db.Rooms.Remove(room);
            await _db.SaveChangesAsync();
            return true;
        }
    }
}
