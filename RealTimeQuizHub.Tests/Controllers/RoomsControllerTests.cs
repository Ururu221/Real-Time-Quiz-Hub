using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using RealTimeQuizHub.Controllers;
using RealTimeQuizHub.Models;
using RealTimeQuizHub.Models.Dtos;
using RealTimeQuizHub.Repository.Interfaces;
using RealTimeQuizHub.Services;

namespace RealTimeQuizHub.Tests.Controllers
{
    public class RoomsControllerTests
    {
        // ===== In-memory fakes (no database) =====

        private class FakeQuestionRepository : IQuestionRepository
        {
            private readonly List<Question> _questions;
            public FakeQuestionRepository(params int[] ids)
            {
                _questions = ids.Select(i => new Question { Id = i, Name = $"Q{i}" }).ToList();
            }

            public Task<List<Question>> GetAllAsync() => Task.FromResult(_questions.ToList());
            public Task<Question> GetQuestionByIdAsync(int questionId)
                => Task.FromResult(_questions.FirstOrDefault(q => q.Id == questionId)!);
            public Task<bool> AddQuestionAsync(Question question) => Task.FromResult(true);
            public Task<bool> UpdateQuestionAsync(Question question) => Task.FromResult(true);
            public Task<bool> DeleteQuestionAsync(int questionId) => Task.FromResult(true);
        }

        private class FakeQuizRepository : IQuizRepository
        {
            private readonly List<Quiz> _quizzes = new();
            private int _nextQuizId = 1;
            private int _nextLinkId = 1;

            public Task<List<Quiz>> GetAllAsync() => Task.FromResult(_quizzes.ToList());

            public Task<Quiz?> GetByIdAsync(int id)
                => Task.FromResult(_quizzes.FirstOrDefault(q => q.Id == id));

            public Task<Quiz> AddAsync(Quiz quiz)
            {
                quiz.Id = _nextQuizId++;
                foreach (var qq in quiz.QuizQuestions)
                {
                    qq.Id = _nextLinkId++;
                    qq.QuizId = quiz.Id;
                }
                _quizzes.Add(quiz);
                return Task.FromResult(quiz);
            }

            public Task<bool> ExistsAsync(int id) => Task.FromResult(_quizzes.Any(q => q.Id == id));

            public Task<bool> DeleteAsync(int id)
            {
                var quiz = _quizzes.FirstOrDefault(q => q.Id == id);
                if (quiz == null) return Task.FromResult(false);
                _quizzes.Remove(quiz);
                return Task.FromResult(true);
            }
        }

        private class FakeRoomRepository : IRoomRepository
        {
            private readonly List<Room> _rooms = new();
            private readonly FakeQuizRepository _quizRepo;
            private int _nextId = 1;

            public FakeRoomRepository(FakeQuizRepository quizRepo) => _quizRepo = quizRepo;

            public Task<List<Room>> GetAllActiveAsync()
            {
                var active = _rooms.Where(r => r.IsActive).ToList();
                foreach (var r in active) r.Quiz = _quizRepo.GetByIdAsync(r.QuizId).Result;
                return Task.FromResult(active);
            }

            public Task<Room?> GetByIdAsync(int id)
            {
                var room = _rooms.FirstOrDefault(r => r.Id == id);
                if (room != null) room.Quiz = _quizRepo.GetByIdAsync(room.QuizId).Result;
                return Task.FromResult(room);
            }

            public Task<Room> AddAsync(Room room)
            {
                room.Id = _nextId++;
                _rooms.Add(room);
                return Task.FromResult(room);
            }

            public Task<bool> DeleteAsync(int id)
            {
                var room = _rooms.FirstOrDefault(r => r.Id == id);
                if (room == null) return Task.FromResult(false);
                _rooms.Remove(room);
                return Task.FromResult(true);
            }
        }

        // ===== Helpers =====

        private static FakeRoomRepository _roomRepo = null!;
        private static FakeQuizRepository _quizRepo = null!;

        private static RoomsController CreateRoomsController(bool isAdmin)
        {
            _quizRepo = new FakeQuizRepository();
            _roomRepo = new FakeRoomRepository(_quizRepo);
            var questionRepo = new FakeQuestionRepository(1, 2, 3);
            var quizService = new QuizService(_quizRepo, questionRepo);
            var roomService = new RoomService(_roomRepo, _quizRepo, quizService);

            var controller = new RoomsController(roomService, NullLogger<RoomsController>.Instance);
            AttachUser(controller, isAdmin);
            return controller;
        }

        private static QuizzesController CreateQuizzesController(bool isAdmin)
        {
            _quizRepo = new FakeQuizRepository();
            var questionRepo = new FakeQuestionRepository(1, 2, 3);
            var quizService = new QuizService(_quizRepo, questionRepo);

            var controller = new QuizzesController(quizService, NullLogger<QuizzesController>.Instance);
            AttachUser(controller, isAdmin);
            return controller;
        }

        private static void AttachUser(ControllerBase controller, bool isAdmin, int userId = 1)
        {
            var claims = new List<Claim>
            {
                new Claim("id", userId.ToString()),
                new Claim("isAdmin", isAdmin.ToString())
            };
            var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, "Test"));
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };
        }

        private static CreateRoomDto NewRoomWithInlineQuiz() => new()
        {
            Name = "History Room",
            Description = "Test your history knowledge",
            NewQuiz = new CreateQuizDto
            {
                Title = "History 101",
                QuestionIds = new List<int> { 1, 2, 3 }
            },
            HasTimer = true,
            TimerSecondsPerQuestion = 30,
            TimerScoreImpact = 0.5
        };

        // ===== TESTS =====

        [Fact]
        public async Task CreateRoom_AsAdmin_Succeeds()
        {
            var controller = CreateRoomsController(isAdmin: true);

            var result = await controller.Create(NewRoomWithInlineQuiz());

            var created = Assert.IsType<CreatedAtActionResult>(result);
            var room = Assert.IsType<RoomDto>(created.Value);
            Assert.Equal("History Room", room.Name);
            Assert.Equal(3, room.QuestionCount);
            Assert.Equal("History 101", room.QuizTitle);
            Assert.True(room.HasTimer);
        }

        [Fact]
        public async Task CreateRoom_AsNonAdmin_Returns403()
        {
            var controller = CreateRoomsController(isAdmin: false);

            var result = await controller.Create(NewRoomWithInlineQuiz());

            var status = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status403Forbidden, status.StatusCode);
        }

        [Fact]
        public async Task GetAll_ReturnsListOfRooms()
        {
            var controller = CreateRoomsController(isAdmin: true);
            await controller.Create(NewRoomWithInlineQuiz());

            var result = await controller.GetAll();

            var ok = Assert.IsType<OkObjectResult>(result);
            var rooms = Assert.IsAssignableFrom<List<RoomDto>>(ok.Value);
            Assert.Single(rooms);
            Assert.Equal("History Room", rooms[0].Name);
        }

        [Fact]
        public async Task CreateQuiz_WithQuestions_Succeeds()
        {
            var controller = CreateQuizzesController(isAdmin: true);

            var result = await controller.Create(new CreateQuizDto
            {
                Title = "Geography",
                QuestionIds = new List<int> { 1, 2 }
            });

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(ok.Value);
        }
    }
}
