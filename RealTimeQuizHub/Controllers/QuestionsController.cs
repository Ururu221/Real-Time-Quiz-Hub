using Microsoft.AspNetCore.Mvc;
using RealTimeQuizHub.Services.Interfaces;

namespace RealTimeQuizHub.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class QuestionsController : ControllerBase
    {
        private readonly IJsonQuestionService _questionService;
        private readonly ILogger<QuizController> _logger;

        public QuestionsController(IJsonQuestionService questionService, ILogger<QuizController> logger)
        {
            _questionService = questionService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllQuestionsAsync()
        {
            var questions = await _questionService.GetAllAsync();
            _logger.LogInformation("Uploaded all questions.");
            return Ok(questions);
        }
    }
}
