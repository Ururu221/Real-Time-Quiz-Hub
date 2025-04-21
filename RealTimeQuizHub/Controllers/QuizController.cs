using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RealTimeQuizHub.Services.Interfaces;

namespace RealTimeQuizHub.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class QuizController : ControllerBase
    {
        private readonly IQuizSessionService _quizSessionService;
        private readonly IQuestionService _questionService;
        private readonly ILogger<QuizController> _logger;
        public QuizController(IQuizSessionService quizSessionService, IQuestionService questionService, ILogger<QuizController> logger)
        {
            _quizSessionService = quizSessionService;
            _questionService = questionService;
            _logger = logger;
        }

        [HttpPost("start")]
        public async Task<IActionResult> StartQuizAsync([FromBody] string quizId)
        {
            var session = await _quizSessionService.StartQuizAsync(quizId);
            _logger.LogInformation($"Quiz session started with ID: {quizId}");
            return Ok(session);
        }

        [HttpGet("{quizId}/next")]
        public async Task<IActionResult> GetNextQuestionAsync(string quizId)
        {
            var question = await _quizSessionService.GetNextQuestionAsync(quizId);
            if (question == null)
            {
                return NotFound("No more questions available.");
            }
            _logger.LogInformation($"Next question for quiz ID {quizId} retrieved.");
            return Ok(question);
        }

        [HttpPost("{quizId}/submit")]
        public async Task<IActionResult> SubmitAnswerAsync(string quizId, [FromBody] string answer)
        {
            var isCorrect = await _quizSessionService.SubmitAnswerAsync(quizId, answer);
            if (isCorrect)
            {
                _logger.LogInformation($"Correct answer submitted for quiz ID {quizId}.");
                return Ok("Correct answer!");
            }
            _logger.LogWarning($"Incorrect answer submitted for quiz ID {quizId}.");
            return BadRequest("Incorrect answer.");
        }

        [HttpGet("{quizId}")]
        public async Task<IActionResult> GetQuizSessionAsync(string quizId)
        {
            var session = await _quizSessionService.GetQuizSessionAsync(quizId);
            if (session == null)
            {
                return NotFound("Quiz session not found.");
            }
            _logger.LogInformation($"Quiz session retrieved for ID: {quizId}");
            return Ok(session);
        }

        [HttpPost("{quizId}/end")]
        public async Task<IActionResult> EndQuizAsync(string quizId)
        {
            await _quizSessionService.EndQuizAsync(quizId);
            _logger.LogWarning($"Quiz session ended for ID: {quizId}");
            return Ok("Quiz ended successfully.");
        }

        [HttpGet("questions")]
        public async Task<IActionResult> GetAllQuestionsAsync()
        {
            var questions = await _questionService.GetAllAsync();
            _logger.LogInformation("Uploaded all questions.");
            return Ok(questions);
        }

        [HttpGet("questions/{questionId}")]
        public async Task<IActionResult> GetQuestionByIdAsync(int questionId)
        {
            var question = await _questionService.GetQuestionByIdAsync(questionId);
            if (question == null)
            {
                return NotFound("Question not found.");
            }
            _logger.LogInformation($"Question retrieved with ID: {questionId}");
            return Ok(question);
        }
    }
}
