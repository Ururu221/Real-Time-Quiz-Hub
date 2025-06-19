using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RealTimeQuizHub.Models;
using RealTimeQuizHub.Models.Dtos;
using RealTimeQuizHub.Services;
using RealTimeQuizHub.Services.Interfaces;

namespace RealTimeQuizHub.Controllers
{
    [ApiController]
    [Route("api/admin/questions")]
    //[Authorize(Policy = "AdminOnly")]
    public class AdminQuestionsController : ControllerBase
    {
        private readonly IQuestionService _questionService;
        private readonly IAnswerService _answerService;
        public AdminQuestionsController(IQuestionService questionService, IAnswerService answerService)
        {
            _questionService = questionService;
            _answerService = answerService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var questions = await _questionService.GetAllQuestionsAsync();
                return Ok(questions);
            }
            catch (System.Exception)
            {
                return StatusCode(500, "Ошибка сервера");
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var question = await _questionService.GetQuestionByIdAsync(id);
                if (question == null)
                {
                    return NotFound("Вопрос не найден");
                }
                // Получаем также варианты ответов для этого вопроса
                List<Answer> answers = await _answerService.GetAnswersByQuestionIdAsync(id);
                return Ok(new { id = question.Id, name = question.Name, answers = answers });
            }
            catch (System.Exception)
            {
                return StatusCode(500, "Ошибка сервера");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] QuestionDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                if (string.IsNullOrWhiteSpace(dto.Text))
                {
                    return BadRequest("Текст вопроса не может быть пустым");
                }
                var question = new Question
                {
                    Name = dto.Text,
                    Answers = new List<Answer>()
                };
                var created = await _questionService.AddQuestionAsync(question);
                return Ok(created);
            }
            catch (System.Exception)
            {
                return StatusCode(500, "Ошибка сервера");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] QuestionDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                if (string.IsNullOrWhiteSpace(dto.Text))
                {
                    return BadRequest("Текст вопроса не может быть пустым");
                }
                bool success = await _questionService.UpdateQuestionAsync(id, dto.Text);
                if (!success)
                {
                    return NotFound("Вопрос не найден");
                }
                return Ok("Вопрос обновлен");
            }
            catch (System.Exception)
            {
                return StatusCode(500, "Ошибка сервера");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                bool success = await _questionService.DeleteQuestionAsync(id);
                if (!success)
                {
                    return NotFound("Вопрос не найден");
                }
                return NoContent();
            }
            catch (System.Exception)
            {
                return StatusCode(500, "Ошибка сервера");
            }
        }
    }
}
