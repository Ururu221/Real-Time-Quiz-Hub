using RealTimeQuizHub.Models;
using RealTimeQuizHub.Models.Dtos;
using RealTimeQuizHub.Repository.Interfaces;
using RealTimeQuizHub.Services.Interfaces;

namespace RealTimeQuizHub.Services
{
    public class RoomService : IRoomService
    {
        private readonly IRoomRepository _roomRepository;
        private readonly IQuizRepository _quizRepository;
        private readonly IQuizService _quizService;

        public RoomService(IRoomRepository roomRepository,
                           IQuizRepository quizRepository,
                           IQuizService quizService)
        {
            _roomRepository = roomRepository;
            _quizRepository = quizRepository;
            _quizService = quizService;
        }

        public async Task<List<RoomDto>> GetActiveRoomsAsync()
        {
            var rooms = await _roomRepository.GetAllActiveAsync();
            return rooms.Select(ToDto).ToList();
        }

        public async Task<RoomDetailDto?> GetRoomDetailAsync(int id)
        {
            var room = await _roomRepository.GetByIdAsync(id);
            if (room == null)
            {
                return null;
            }

            var detail = new RoomDetailDto();
            CopyInto(room, detail);
            detail.Questions = (room.Quiz?.QuizQuestions ?? new List<QuizQuestion>())
                .OrderBy(qq => qq.OrderIndex)
                .Select(qq => new QuizQuestionDto
                {
                    QuestionId = qq.QuestionId,
                    Name = qq.Question?.Name ?? string.Empty,
                    OrderIndex = qq.OrderIndex
                })
                .ToList();
            return detail;
        }

        public async Task<RoomDto> CreateRoomAsync(CreateRoomDto dto, int createdByUserId)
        {
            if (dto == null)
            {
                throw new ArgumentNullException(nameof(dto));
            }
            if (string.IsNullOrWhiteSpace(dto.Name))
            {
                throw new ArgumentException("Room name cannot be empty.", nameof(dto));
            }
            if (dto.TimerScoreImpact < 0.0 || dto.TimerScoreImpact > 1.0)
            {
                throw new ArgumentException("Timer score impact must be between 0.0 and 1.0.", nameof(dto));
            }

            // Resolve which quiz the room will host: existing one or a freshly created inline quiz.
            int quizId;
            if (dto.QuizId.HasValue)
            {
                if (!await _quizRepository.ExistsAsync(dto.QuizId.Value))
                {
                    throw new ArgumentException($"Quiz with ID {dto.QuizId.Value} does not exist.", nameof(dto));
                }
                quizId = dto.QuizId.Value;
            }
            else if (dto.NewQuiz != null)
            {
                var quiz = await _quizService.CreateQuizAsync(dto.NewQuiz, createdByUserId);
                quizId = quiz.Id;
            }
            else
            {
                throw new ArgumentException("A room must reference an existing quiz or include a new quiz.", nameof(dto));
            }

            var room = new Room
            {
                Name = dto.Name.Trim(),
                Description = dto.Description,
                CreatedByUserId = createdByUserId,
                QuizId = quizId,
                IsActive = true,
                HasTimer = dto.HasTimer,
                TimerSecondsPerQuestion = dto.HasTimer ? dto.TimerSecondsPerQuestion : 0,
                TimerScoreImpact = dto.HasTimer ? dto.TimerScoreImpact : 0.0,
                CreatedAt = DateTime.UtcNow
            };

            var created = await _roomRepository.AddAsync(room);

            // Reload so the returned DTO carries quiz title / question count.
            var full = await _roomRepository.GetByIdAsync(created.Id);
            return ToDto(full ?? created);
        }

        public Task<bool> DeleteRoomAsync(int id)
        {
            return _roomRepository.DeleteAsync(id);
        }

        private static RoomDto ToDto(Room room)
        {
            var dto = new RoomDto();
            CopyInto(room, dto);
            return dto;
        }

        private static void CopyInto(Room room, RoomDto dto)
        {
            dto.Id = room.Id;
            dto.Name = room.Name;
            dto.Description = room.Description;
            dto.QuizId = room.QuizId;
            dto.QuizTitle = room.Quiz?.Title ?? string.Empty;
            dto.QuestionCount = room.Quiz?.QuizQuestions?.Count ?? 0;
            dto.IsActive = room.IsActive;
            dto.HasTimer = room.HasTimer;
            dto.TimerSecondsPerQuestion = room.TimerSecondsPerQuestion;
            dto.TimerScoreImpact = room.TimerScoreImpact;
            dto.CreatedByUserId = room.CreatedByUserId;
            dto.CreatedAt = room.CreatedAt;
        }
    }
}
