using Microsoft.EntityFrameworkCore;
using RealTimeQuizHub.Models;
using RealTimeQuizHub.Repository.Interfaces;

namespace RealTimeQuizHub.Repository
{
    public class QuizRepository : IQuizRepository
    {
        private readonly AppDbContext _db;
        public QuizRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task<List<Quiz>> GetAllAsync()
        {
            return await _db.Quizzes
                .Include(q => q.QuizQuestions)
                .OrderByDescending(q => q.CreatedAt)
                .ToListAsync();
        }

        public async Task<Quiz?> GetByIdAsync(int id)
        {
            return await _db.Quizzes
                .Include(q => q.QuizQuestions)
                    .ThenInclude(qq => qq.Question!)
                        .ThenInclude(qn => qn.Answers)
                .AsSplitQuery()
                .FirstOrDefaultAsync(q => q.Id == id);
        }

        public async Task<Quiz> AddAsync(Quiz quiz)
        {
            _db.Quizzes.Add(quiz);
            await _db.SaveChangesAsync();
            return quiz;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var quiz = await _db.Quizzes.FindAsync(id);
            if (quiz == null)
            {
                return false;
            }
            // Cascade deletes the quiz_questions links (and any rooms hosting it).
            _db.Quizzes.Remove(quiz);
            await _db.SaveChangesAsync();
            return true;
        }

        public Task<bool> ExistsAsync(int id)
        {
            return _db.Quizzes.AnyAsync(q => q.Id == id);
        }
    }
}
