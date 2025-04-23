using Microsoft.EntityFrameworkCore;
using RealTimeQuizHub.Models;
using RealTimeQuizHub.Repository.Interfaces;

namespace RealTimeQuizHub.Repository
{
    public class QuestionRepository : IQuestionRepository
    {
        private readonly AppDbContext _db;
        public QuestionRepository(AppDbContext db)
        {
            _db = db;
        }
        public async Task<bool> AddQuestionAsync(Question question)
        {
            if (question == null)
            {
                return false;
            }
            _db.Questions.Add(question);
            await _db.SaveChangesAsync();
            return true;
        }
        public async Task<bool> DeleteQuestionAsync(int questionId)
        {
            var question = await _db.Questions.FindAsync(questionId);
            if (question == null)
            {
                return false;
            }
            _db.Questions.Remove(question);
            await _db.SaveChangesAsync();
            return true;
        }
        public async Task<List<Question>> GetAllAsync()
        {
            return await _db.Questions.ToListAsync();
        }
        public async Task<Question> GetQuestionByIdAsync(int questionId)
        {
            var question = await _db.Questions.FindAsync(questionId);
            if (question == null)
            {
                return null;
            }
            return question;
        }
        public async Task<bool> UpdateQuestionAsync(Question question)
        {
            if (question == null)
            {
                return false;
            }
            _db.Questions.Update(question);
            await _db.SaveChangesAsync();
            return true;
        }
    }
}
