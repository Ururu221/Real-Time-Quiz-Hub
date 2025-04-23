using Microsoft.AspNetCore.Identity;
using RealTimeQuizHub.Models;
using RealTimeQuizHub.Models.Dtos;
using RealTimeQuizHub.Repository.Interfaces;
using RealTimeQuizHub.Services.Interfaces;
using System.Reflection.Metadata.Ecma335;
using System.Xml.Linq;

namespace RealTimeQuizHub.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher<User> _hasher;

        public UserService(IUserRepository userRepository, IPasswordHasher<User> hasher)
        {
            _userRepository = userRepository;
            _hasher = hasher;
        }

        public async Task<User> AddUserAsync(RegisterDto dto)
        {
            if (string.IsNullOrEmpty(dto.Name) || string.IsNullOrEmpty(dto.Email) || string.IsNullOrEmpty(dto.Password))
            {
                throw new ArgumentException("Email and password must be provided.");
            }

            var user = new User
            {
                Name = dto.Name,
                Email = dto.Email,
                IsAdmin = dto.IsAdmin
            };

            user.PasswordHash = _hasher.HashPassword(user, dto.Password);

            await _userRepository.AddUserAsync(user);

            return user;
        }

        public Task<bool> DeleteUserAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentException("User ID cannot be null or empty.", nameof(userId));
            }
            return _userRepository.DeleteUserAsync(userId);
        }

        public Task<List<User>> GetAllUsersAsync()
        {
            return _userRepository.GetAllUsersAsync();
        }

        public Task<User> GetUserByEmailAsync(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                throw new ArgumentException("Email cannot be null or empty.", nameof(email));
            }
            return _userRepository.GetUserByEmailAsync(email);
        }

        public Task<User> GetUserByIdAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentException("User ID cannot be null or empty.", nameof(userId));
            }
            return _userRepository.GetUserByIdAsync(userId);
        }

        public Task<User> UpdateUserAsync(User user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user), "User cannot be null.");
            }
            return _userRepository.UpdateUserAsync(user);
        }

        // returns true if user exists, false otherwise
        public async Task<bool> UserExists(string email) 
        {
            if (string.IsNullOrEmpty(email))
            {
                throw new ArgumentException("Email cannot be null or empty.", nameof(email));
            }
            var user = await _userRepository.GetUserByEmailAsync(email);
            return user != null;
        }

        public async Task<User?> ValidateCredentials(string email, string password)
        {
            // check if email and password are not null or empty
            var user = await _userRepository.GetUserByEmailAsync(email);
            if (user == null) return null;

            // check if password is correct
            var result = _hasher.VerifyHashedPassword(user, user.PasswordHash, password);
            return result == PasswordVerificationResult.Success ? user : null;
        }
    }
}
