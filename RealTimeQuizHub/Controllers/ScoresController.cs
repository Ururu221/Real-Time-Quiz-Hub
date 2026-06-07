using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RealTimeQuizHub.Models.Dtos;
using RealTimeQuizHub.Services.Interfaces;

namespace RealTimeQuizHub.Controllers
{
    [ApiController]
    [Route("api/scores")]
    [Authorize]
    public class ScoresController : ControllerBase
    {
        private readonly IScoreService _scoreService;
        private readonly ILogger<ScoresController> _logger;

        public ScoresController(IScoreService scoreService, ILogger<ScoresController> logger)
        {
            _scoreService = scoreService;
            _logger = logger;
        }

        // POST /api/scores/complete — record a finished quiz for the current user.
        // The score is computed server-side from the per-answer data in the body.
        [HttpPost("complete")]
        public async Task<IActionResult> Complete([FromBody] CompleteQuizDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = GetUserId();
            if (userId == 0)
            {
                return Unauthorized(new { message = "Не вдалося визначити користувача." });
            }

            try
            {
                var result = await _scoreService.RecordCompletionAsync(userId, dto);
                _logger.LogInformation(
                    "User {UserId} completed quiz {QuizId}: {Score} pts, rank {Rank}",
                    userId, dto.QuizId, result.Score, result.Rank);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        private int GetUserId()
        {
            var claim = User.FindFirst("id")?.Value;
            return int.TryParse(claim, out var id) ? id : 0;
        }
    }
}
