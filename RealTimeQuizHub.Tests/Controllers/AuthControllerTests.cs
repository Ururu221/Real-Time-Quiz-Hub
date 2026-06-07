using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using RealTimeQuizHub.Controllers;
using RealTimeQuizHub.Models;
using RealTimeQuizHub.Models.Dtos;
using RealTimeQuizHub.Repository.Interfaces;
using RealTimeQuizHub.Services;

namespace RealTimeQuizHub.Tests.Controllers
{
    public class AuthControllerTests
    {
        // In-memory fake so registration/login run through the real
        // UserService, PasswordHasher and JwtGenerator without a database.
        private class InMemoryUserRepository : IUserRepository
        {
            private readonly List<User> _users = new();
            private int _nextId = 1;

            public Task<bool> AddUserAsync(User user)
            {
                if (user == null) return Task.FromResult(false);
                user.Id = _nextId++;
                _users.Add(user);
                return Task.FromResult(true);
            }

            public Task<User> GetUserByEmailAsync(string email)
                => Task.FromResult(_users.SingleOrDefault(u => u.Email == email)!);

            public Task<User> GetUserByIdAsync(string userId)
                => Task.FromResult(_users.SingleOrDefault(u => u.Id.ToString() == userId)!);

            public Task<List<User>> GetAllUsersAsync() => Task.FromResult(_users.ToList());

            public Task<User> UpdateUserAsync(User user) => Task.FromResult(user);

            public Task<bool> DeleteUserAsync(string userId) => Task.FromResult(true);
        }

        private static AuthController CreateController()
        {
            var repo = new InMemoryUserRepository();
            var hasher = new PasswordHasher<User>();
            var userService = new UserService(repo, hasher);

            var jwtSettings = Options.Create(new JwtSettings
            {
                Secret = "TestSecretKeyForUnitTestsThatIsLongEnough123456",
                Issuer = "RealTimeQuizTest",
                Audience = "TestAudience",
                ExpiresInMinutes = 60
            });
            var jwtGen = new JwtGenerator(jwtSettings);

            return new AuthController(userService, jwtGen);
        }

        private static RegisterDto NewUser(string email = "alice@example.com") => new()
        {
            Name = "Alice",
            Email = email,
            Password = "secret123",
            IsAdmin = false
        };

        // ===== REGISTRATION =====

        [Fact]
        public async Task Register_WithNewEmail_ReturnsOkWithToken()
        {
            var controller = CreateController();

            var result = await controller.Register(NewUser());

            var ok = Assert.IsType<OkObjectResult>(result);
            var body = Assert.IsType<AuthResponseDto>(ok.Value);
            Assert.False(string.IsNullOrWhiteSpace(body.Token));
            Assert.Equal("alice@example.com", body.User.Email);
            Assert.Equal("Alice", body.User.Name);
        }

        [Fact]
        public async Task Register_WithDuplicateEmail_ReturnsConflict()
        {
            var controller = CreateController();

            var first = await controller.Register(NewUser());
            Assert.IsType<OkObjectResult>(first);

            // Same email a second time must fail.
            var second = await controller.Register(NewUser());

            Assert.IsType<ConflictObjectResult>(second);
        }

        // ===== LOGIN =====

        [Fact]
        public async Task Login_WithCorrectCredentials_ReturnsOkWithToken()
        {
            var controller = CreateController();
            await controller.Register(NewUser());

            var result = await controller.Login(new LoginDto
            {
                Email = "alice@example.com",
                Password = "secret123"
            });

            var ok = Assert.IsType<OkObjectResult>(result);
            var body = Assert.IsType<AuthResponseDto>(ok.Value);
            Assert.False(string.IsNullOrWhiteSpace(body.Token));
            Assert.Equal("alice@example.com", body.User.Email);
        }

        [Fact]
        public async Task Login_WithWrongPassword_ReturnsUnauthorized()
        {
            var controller = CreateController();
            await controller.Register(NewUser());

            var result = await controller.Login(new LoginDto
            {
                Email = "alice@example.com",
                Password = "wrong-password"
            });

            Assert.IsType<UnauthorizedObjectResult>(result);
        }
    }
}
