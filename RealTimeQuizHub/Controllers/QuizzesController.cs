using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RealTimeQuizHub.Models.Dtos;
using RealTimeQuizHub.Services.Interfaces;

namespace RealTimeQuizHub.Controllers
{
    [ApiController]
    [Route("api/quizzes")]
    [Authorize]
    public class QuizzesController : ControllerBase
    {
        private readonly IQuizService _quizService;
        private readonly ILogger<QuizzesController> _logger;

        public QuizzesController(IQuizService quizService, ILogger<QuizzesController> logger)
        {
            _quizService = quizService;
            _logger = logger;
        }

        // GET /api/quizzes — public list of quizzes (any authenticated user).
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var quizzes = await _quizService.GetAllAsync();
            return Ok(quizzes);
        }

        // GET /api/quizzes/{id} — quiz summary (used by the gameplay page for timer settings).
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var quiz = await _quizService.GetByIdAsync(id);
            if (quiz == null)
            {
                return NotFound(new { message = "Вікторину не знайдено." });
            }
            return Ok(quiz);
        }

        // POST /api/quizzes — create a quiz with new questions and answers (admins only).
        [HttpPost]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Create([FromBody] CreateQuizDto dto)
        {
            if (!IsAdmin())
            {
                return StatusCode(StatusCodes.Status403Forbidden,
                    new { message = "Лише адміністратори можуть створювати вікторини." });
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var quiz = await _quizService.CreateQuizAsync(dto, GetUserId());
                _logger.LogInformation("Quiz {QuizId} created by user {UserId}", quiz.Id, GetUserId());
                return Ok(new
                {
                    id = quiz.Id,
                    title = quiz.Title,
                    description = quiz.Description,
                    questionCount = quiz.QuizQuestions.Count,
                    hasTimer = quiz.HasTimer,
                    timerSecondsPerQuestion = quiz.TimerSecondsPerQuestion,
                    timerScoreImpact = quiz.TimerScoreImpact
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // DELETE /api/quizzes/{id} — remove a quiz (admins only).
        [HttpDelete("{id}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Delete(int id)
        {
            if (!IsAdmin())
            {
                return StatusCode(StatusCodes.Status403Forbidden,
                    new { message = "Лише адміністратори можуть видаляти вікторини." });
            }

            var deleted = await _quizService.DeleteQuizAsync(id);
            if (!deleted)
            {
                return NotFound(new { message = "Вікторину не знайдено." });
            }
            _logger.LogInformation("Quiz {QuizId} deleted by user {UserId}", id, GetUserId());
            return NoContent();
        }

        private bool IsAdmin()
        {
            var claim = User.FindFirst("isAdmin")?.Value;
            return string.Equals(claim, "true", StringComparison.OrdinalIgnoreCase);
        }

        private int GetUserId()
        {
            var claim = User.FindFirst("id")?.Value;
            return int.TryParse(claim, out var id) ? id : 0;
        }
    }
}
