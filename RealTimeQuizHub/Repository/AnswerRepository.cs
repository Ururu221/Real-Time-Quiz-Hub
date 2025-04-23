using Microsoft.EntityFrameworkCore;
using RealTimeQuizHub.Models;
using RealTimeQuizHub.Repository.Interfaces;

namespace RealTimeQuizHub.Repository
{
    public class AnswerRepository : IAnswerRepository
    {
        private readonly AppDbContext _db;

        public AnswerRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task<bool> AddAnswerAsync(Answer answer)
        {
            if (answer == null)
            {
                return false;
            }
            _db.Answers.Add(answer);
            await _db.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeleteAnswerAsync(int Id)
        {
            var answer = await _db.Answers.FindAsync(Id);
            if (answer == null)
            {
                return false;
            }
            _db.Answers.Remove(answer);
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<List<Answer>> GetAllAsync()
        {
            return await _db.Answers.ToListAsync();
        }

        public async Task<Answer> GetAnswerByIdAsync(int answerId)
        {
            var answer = await _db.Answers.FindAsync(answerId);
            if (answer == null)
            {
                return null;
            }
            return answer;
        }

        public async Task<bool> UpdateAnswerAsync(Answer answer)
        {
            if (answer == null)
            {
                return false;
            }
            _db.Answers.Update(answer);
            await _db.SaveChangesAsync();
            return true;
        }
    }
}
