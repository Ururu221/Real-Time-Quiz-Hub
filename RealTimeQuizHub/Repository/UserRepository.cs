using Microsoft.EntityFrameworkCore;
using RealTimeQuizHub.Models;
using RealTimeQuizHub.Repository.Interfaces;

namespace RealTimeQuizHub.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _db;
        public UserRepository(AppDbContext db)
        {
            _db = db;
        }
        public async Task<User> GetUserByIdAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return null;
            }
            return await _db.Users.FindAsync(userId);
        }
        public async Task<User> GetUserByEmailAsync(string email)
        {
            return await _db.Users
                .SingleOrDefaultAsync(u => u.Email == email);
        }
        public Task<List<User>> GetAllUsersAsync()
        {
            return _db.Users.ToListAsync();
        }
        public async Task<bool> AddUserAsync(User user)
        {
            if (user == null)
            {
                return false;
            }
            _db.Users.Add(user);
            await _db.SaveChangesAsync();
            return true;
        }
        public async Task<User> UpdateUserAsync(User user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user), "User cannot be null.");
            }
            _db.Users.Update(user);
            await _db.SaveChangesAsync();
            return user;
        }
        public async Task<bool> DeleteUserAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentException("User ID cannot be null or empty.", nameof(userId));
            }
            var user = await _db.Users.FindAsync(userId);
            if (user == null)
            {
                throw new KeyNotFoundException($"User with ID {userId} not found.");
            }
            _db.Users.Remove(user);
            await _db.SaveChangesAsync();
            return true;
        }
    }
}
